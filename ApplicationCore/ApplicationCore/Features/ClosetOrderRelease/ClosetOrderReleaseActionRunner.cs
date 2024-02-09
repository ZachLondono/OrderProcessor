using ApplicationCore.Features.ClosetOrderSelector;
using ApplicationCore.Shared;
using ApplicationCore.Shared.Services;
using Domain.Components.ProgressModal;
using Domain.Services;
using System.Diagnostics;
using Domain.Excel;
using QuestPDF.Fluent;
using UglyToad.PdfPig.Writer;
using Microsoft.Extensions.Logging;
using Microsoft.Office.Interop.Outlook;
using Domain.Extensions;
using System.Runtime.InteropServices;
using MimeKit;
using Exception = System.Exception;
using Action = System.Action;
using OutlookApp = Microsoft.Office.Interop.Outlook.Application;
using ExcelApp = Microsoft.Office.Interop.Excel.Application;
using Range = Microsoft.Office.Interop.Excel.Range;
using Microsoft.Office.Interop.Excel;
using OrderExporting.CNC.ReleasePDF;
using OrderExporting.CNC.Programs.WSXML;
using OrderExporting.CNC.Programs.Job;
using OrderExporting.CNC.Programs.WorkOrderReleaseEmail;
using OrderExporting.CNC.Programs.WSXML.Report;
using OrderExporting.Shared;

namespace ApplicationCore.Features.ClosetOrderRelease;

public class ClosetOrderReleaseActionRunner(ILogger<ClosetOrderReleaseActionRunner> logger, CNCReleaseDecoratorFactory releaseDecoratorFactory, IFileReader fileReader, IEmailService emailService, IWSXMLParser wsxmlParser, IWindowFocuser windowFocuser)
        : IActionRunner {

    public Action? ShowProgressBar { get; set; }
    public Action? HideProgressBar { get; set; }
    public Action<int>? SetProgressBarValue { get; set; }
    public Action<ProgressLogMessage>? PublishProgressMessage { get; set; }

    public ClosetOrder? ClosetOrder { get; set; }
    public ClosetOrderReleaseOptions? Options { get; set; }

    private readonly ILogger<ClosetOrderReleaseActionRunner> _logger = logger;
    private readonly CNCReleaseDecoratorFactory _releaseDecoratorFactory = releaseDecoratorFactory;
    private readonly IFileReader _fileReader = fileReader;
    private readonly IEmailService _emailService = emailService;
    private readonly IWSXMLParser _wsxmlParser = wsxmlParser;
    private readonly IWindowFocuser _windowFocuser = windowFocuser;

    public async Task Run() {

        if (ClosetOrder is null || Options is null) {
            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "Closet order or release options not set"));
            return;
        }

        var outputDirectories = Options.OutputDirectory.Split(';');
        if (outputDirectories.Length == 0) {
            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "No output directories set"));
            return;
        }

        Document? cncDocument = null;
        string? excelPdfFilePath = null;
        ReleasedJob? releasedJob = null;

        if (Options.AddExistingWSXMLReport) {

            if (!_fileReader.DoesFileExist(Options.WSXMLReportFilePath)) {
                PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "Can not find WSXML report"));
                return;
            }

            (cncDocument, releasedJob) = CreateCNCReleaseDocument(Options.WSXMLReportFilePath, ClosetOrder.OrderDate, ClosetOrder.DueDate, ClosetOrder.Customer, "Royal Cabinet Company");

            if (cncDocument is null || releasedJob is null) {
                return;
            }

        }

        if (Options.IncludeCover || Options.IncludePackingList) {

            if (!_fileReader.DoesFileExist(Options.WorkbookFilePath)) {
                PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "Can not find Closet Order workbook"));
                return;
            }

            excelPdfFilePath = await GeneratePDFFromWorkbook(Options.IncludeCover, Options.IncludePackingList, Options.IncludePartList, Options.IncludeDBList, Options.IncludeMDFList, Options.WorkbookFilePath, Options.SeperateCoverPDF, Options.SeperatePackingListPDF, Options.SeperatePDFDirectory);
            
            if (excelPdfFilePath is null) {
                return;
            }

        }

        var file = await MergePDFs(outputDirectories, Options.FileName, cncDocument, excelPdfFilePath);

        if (file is null) {

            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "File was not generated"));
            return;
            
        }

        if (Options.SendEmail) {

            List<string> attachments = [];
            if (file is not null) {
                attachments.Add(file);
            }

            var (HTMLBody, TextBody) = GenerateEmailBodies(releasedJob);
            string subject = $"RELEASED: {ClosetOrder.OrderNumber} {ClosetOrder.Customer}";

            if (Options.PreviewEmail) {
                await Task.Run(() => CreateAndDisplayOutlookEmail(Options.EmailRecipients, subject, HTMLBody, TextBody, attachments));
            } else {
                await SendEmail(Options.EmailRecipients, subject, HTMLBody, TextBody, attachments);
            }

        }
        
        if (excelPdfFilePath is not null && _fileReader.DoesFileExist(excelPdfFilePath)) {

            try {

                File.Delete(excelPdfFilePath);

            } catch (Exception ex) {

                _logger.LogError(ex, "Exception thrown while trying to delete temporary excel pdf");

            }

        }

    }

    private async Task<string?> MergePDFs(string[] outputDirectories, string fileName, Document? document, string? excelPdf) {

        if (outputDirectories.Length == 0) {
            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "Output directory not found"));
        }

        var fileComponents = new List<byte[]>();
        
        if (excelPdf is not null) {

            if (_fileReader.DoesFileExist(excelPdf)) {

                try {

                    var data = await File.ReadAllBytesAsync(excelPdf);
    
                    fileComponents.Add(data);

                } catch (Exception ex) {

                    PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "Error occurred reading pdf from excel"));
                    _logger.LogError(ex, "Exception thrown while trying to read pdf file for closet release - {FilePath}", excelPdf);

                }

            } else {

                // TODO: error message, pdf can not be found

            }

        }

        if (document is not null) {

            try {

                var data = document.GeneratePdf();

                fileComponents.Add(data);

            } catch (Exception ex) {

                PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "Error occurred generating CNC Release pdf"));
                _logger.LogError(ex, "Exception thrown while trying to generate CNC release PDF");

            }

        }

        if (fileComponents.Count <= 0) {

            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "Not generating pdf, there are no pages to add."));
            return null;

        }

        try {

            string mergedFilePath = "";

            foreach (var directory in outputDirectories) {

                var mergedDocument = await Task.Run(() => PdfMerger.Merge(fileComponents));
    
                mergedFilePath = _fileReader.GetAvailableFileName(directory, fileName, "pdf");
                
                await File.WriteAllBytesAsync(mergedFilePath, mergedDocument);

                PublishProgressMessage?.Invoke(new(ProgressLogMessageType.FileCreated, mergedFilePath));

            }
            
            return mergedFilePath;

        } catch (Exception ex) {

            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "Error occurred generating closet order release pdf"));
            _logger.LogError(ex, "Exception thrown while trying merge pdfs for closet release");
            return null;

        }

    }

    private (Document?, ReleasedJob?) CreateCNCReleaseDocument(string wsxmlFilePath, DateTime orderDate, DateTime dueDate, string customerName, string vendorName) {

        if (!_fileReader.DoesFileExist(wsxmlFilePath)) {

            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, $"Can not find WSXML report file - {wsxmlFilePath}"));
            return (null, null);

        }

        WSXMLReport? report = null;

        try {

            report = WSXMLParser.ParseWSXMLReport(wsxmlFilePath);

        } catch (Exception ex) {

            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "Error occurred while trying to WSXML report"));
            _logger.LogError(ex, "Exception thrown while trying to parse WSXML report");
            return (null, null);

        }
 
        if (report is null) {

            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "Could not parse WSXML report"));
            return (null, null);

        }

        IDocumentDecorator? decorator = null;
        ReleasedJob? job = null;

        try {

            job = _wsxmlParser.MapDataToReleasedJob(report, orderDate, dueDate, customerName, vendorName);
            decorator = _releaseDecoratorFactory.Create(job);

        } catch (Exception ex) {

            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "Error occurred while trying to assemble CNC release pdf"));
            _logger.LogError(ex, "Exception thrown while trying to create CNC release document decorator");
            return (null, null);

        }
        
        if (decorator is null) {

            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "Could not parse assemble CNC release PDF content"));
            return (null, null);

        }

        var document = Document.Create(decorator.Decorate);

        return (document, job);

    }

    private async Task<string?> GeneratePDFFromWorkbook(bool includeCover, bool includePackingList, bool includePartList, bool includeDBList, bool includeMDFList, string filePath, bool seperateCover, bool seperatePackingList, string seperatePDFDirectory) {

        bool wasOrderOpen = true;
        string? tmpFileName = null;
        await Task.Run(() => {

            var app = GetExcelInstance();

            try {

                if (app is null) {
                    wasOrderOpen = false;
                    return;
                }

                app.ScreenUpdating = false;
                app.DisplayAlerts = false;
                app.Calculation = XlCalculation.xlCalculationManual;

                var workbooks = app.Workbooks;
                var workbook = workbooks.Open(filePath);
                Sheets worksheets = workbook.Worksheets;

                var pdfSheetNames = new List<string>();

                if (includeCover) {
                    const string sheetName = "Cover";

                    Worksheet cover = worksheets[sheetName];
                    cover.PageSetup.PrintArea = $"A1:E48";
                    pdfSheetNames.Add(sheetName);

                    if (seperateCover && Directory.Exists(seperatePDFDirectory)) {
                        try {
                            cover.Outline.ShowLevels(RowLevels:2);
                            cover.PageSetup.FitToPagesTall = 1;
                            string filePath = Path.Combine(seperatePDFDirectory, $"{ClosetOrder?.OrderNumber} Invoice");
                            cover.ExportAsFixedFormat2(XlFixedFormatType.xlTypePDF, filePath, OpenAfterPublish: false);
                            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.FileCreated, filePath + ".pdf"));
                        } catch (Exception ex) {
                            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, $"Error while generating invoice pdf - {ex.Message}"));
                        }
                    }

                    cover.Outline.ShowLevels(RowLevels: 1);

                }

                if (includePackingList) {
                    const string sheetName = "Packing List";
                    SetSheetPrintArea(worksheets, sheetName, "E", "L", 5);
                    pdfSheetNames.Add(sheetName);

                    if (seperatePackingList && Directory.Exists(seperatePDFDirectory)) {
                        try {
                            Worksheet sheet = worksheets[sheetName];
                            string filePath = Path.Combine(seperatePDFDirectory, $"{ClosetOrder?.OrderNumber} Packing List");
                            sheet.ExportAsFixedFormat2(XlFixedFormatType.xlTypePDF, filePath, OpenAfterPublish: false);
                            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.FileCreated, filePath + ".pdf"));
                        } catch (Exception ex) {
                            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, $"Error while generating packing list pdf - {ex.Message}"));
                        }
                    }
                }

                if (includePartList) {

                    const string sheetName = "Closet Parts";
                    SetSheetPrintArea(worksheets, sheetName, "B", "J", 2);
                    pdfSheetNames.Add(sheetName);

                    var cornerSheet = worksheets["Corner Shelves"];
                    var firstRowVal = cornerSheet.Range["B2"]?.Value2?.ToString() ?? "";
                    if (!string.IsNullOrWhiteSpace(firstRowVal)) {
                        SetSheetPrintArea(worksheets, "Corner Shelves", "B", "K", 2);
                        pdfSheetNames.Add(sheetName);
                    }

                    /*
                    var zargenSheet = worksheets["Zargen"];
                    firstRowVal = cornerSheet.Range[""].Value2.ToString();
                    if (!string.IsNullOrWhiteSpace(firstRowVal)) {
                        SetSheetPrintArea(worksheets, "Zargen", "B", "K", 2);
                        pdfSheetNames.Add(sheetName);
                    }
                    */

                }

                if (includeDBList) {
                    const string sheetName = "Dovetail";
                    SetSheetPrintArea(worksheets, sheetName, "B", "J", 17, XlPageOrientation.xlLandscape);
                    pdfSheetNames.Add(sheetName);
                }

                if (includeMDFList) {
                    const string sheetName = "MDF Fronts";
                    SetSheetPrintArea(worksheets, sheetName, "B", "H", 6);
                    pdfSheetNames.Add(sheetName);
                }

                Worksheet previouslyActiveSheet = workbook.ActiveSheet;

                tmpFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".pdf";

                string[] sheetsToSelect = [.. pdfSheetNames];
                worksheets[sheetsToSelect].Select();

                Worksheet activeSheet = workbook.ActiveSheet;
                activeSheet.ExportAsFixedFormat2(XlFixedFormatType.xlTypePDF, tmpFileName, OpenAfterPublish: false);

                previouslyActiveSheet?.Select();

            } finally {

                if (app is not null) {
                    app.ScreenUpdating = true;
                    app.DisplayAlerts = true;
                    app.Calculation = XlCalculation.xlCalculationAutomatic;
                }

                _windowFocuser.TryToSetMainWindowFocus();

            }

        });

        if (!wasOrderOpen) {
            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "Closet order is not open"));
        }

        return tmpFileName;

    }

    private static void SetSheetPrintArea(Sheets worksheets, string sheetName, string checkCol, string lastCol, int startRow, XlPageOrientation orientation = XlPageOrientation.xlPortrait) {

        Worksheet sheet = worksheets[sheetName];
        const int maxRow = 206;
        int lastRow = startRow;

        for (int currentRow = 5; currentRow <= maxRow; currentRow++) {

            Range rng = sheet.Range[$"{checkCol}{currentRow}"];
            var val = rng.Value2?.ToString() ?? "";

            if (!string.IsNullOrWhiteSpace(val) && val != "0") {
                lastRow = currentRow;
            }

        }

        sheet.PageSetup.PrintArea = $"A1:{lastCol}{lastRow}";
        sheet.PageSetup.Orientation = orientation;

    }

    private void CreateAndDisplayOutlookEmail(string recipients, string subject, string htmlBody, string textBody, IEnumerable<string> attachments) {

        var app = new OutlookApp();
        MailItem mailItem = (MailItem)app.CreateItem(OlItemType.olMailItem);
        mailItem.To = recipients;
        mailItem.Subject = subject;
        mailItem.Body = textBody;
        mailItem.HTMLBody = htmlBody;

        attachments.Where(_fileReader.DoesFileExist)
                   .ForEach(att => mailItem.Attachments.Add(att));

        var senderMailBox = _emailService.GetSender();
        var sender = GetSenderOutlookAccount(app, senderMailBox.Address);

        if (sender is not null) {
            mailItem.SendUsingAccount = sender;
        }

        mailItem.Display();

        Marshal.ReleaseComObject(app);
        Marshal.ReleaseComObject(mailItem);
        if (sender is not null) Marshal.ReleaseComObject(sender);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        GC.WaitForPendingFinalizers();

    }

    private static Account? GetSenderOutlookAccount(OutlookApp app, string preferredEmail) {

        var accounts = app.Session.Accounts;
        if (accounts is null || accounts.Count == 0) {
            return null;
        }

        Account? sender = null;
        foreach (Account account in accounts) {
            sender ??= account;
            if (account.SmtpAddress == preferredEmail) {
                sender = account;
                break;
            }
        }

        return sender;

    }

    private async Task SendEmail(string recipients, string subject, string htmlBody, string textBody, IEnumerable<string> attachments) {

        var message = new MimeMessage();

        recipients.Split(';')
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ForEach(r => message.To.Add(new MailboxAddress(r, r)));

        if (message.To.Count == 0) {
            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "No email recipients specified"));
            return;
        }

        var sender = _emailService.GetSender();
        message.From.Add(sender);
        message.Subject = subject;

        var builder = new BodyBuilder {
            TextBody = textBody,
            HtmlBody = htmlBody
        };
        attachments.Where(_fileReader.DoesFileExist).ForEach(att => builder.Attachments.Add(att));

        message.Body = builder.ToMessageBody();

        var response = await Task.Run(() => _emailService.SendMessageAsync(message));
        PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Success, "Email sent"));

    }

    private static (string HTMLBody, string TextBody) GenerateEmailBodies(ReleasedJob? job) {

        var releasedJobs = new List<Job>();

        if (job is not null) {

            var usedMaterials = job.Releases
                                    .First()
                                    .Programs
                                    .Select(p => p.Material)
                                    .GroupBy(m => (m.Name, m.Width, m.Length, m.Thickness, m.IsGrained))
                                    .Select(g => new UsedMaterial(g.Count(), g.Key.Name, g.Key.Width, g.Key.Length, g.Key.Thickness));
    
            releasedJobs.Add(new(job.JobName, usedMaterials));

        }


        var model = new ReleasedWorkOrderSummary(releasedJobs, false, false, false, null);

        var htmlBody = ReleaseEmailBodyGenerator.GenerateHTMLReleaseEmailBody(model, true);
        var textBody = ReleaseEmailBodyGenerator.GenerateTextReleaseEmailBody(model, true);

        return (htmlBody, textBody);

    }

    private static ExcelApp? GetExcelInstance() {

        var allProcesses = Process.GetProcesses();

        foreach (var process in allProcesses) {

            nint winHandle = process.MainWindowHandle;

            var retriever = new ExcelApplicationRetriever((int)winHandle);

            if (retriever.xl is not null) {
                return retriever.xl;
            }

        }

        return null;

    }

}
