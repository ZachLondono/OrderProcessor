using ApplicationCore.Features.MDFDoorOrders.OpenDoorOrders;
using ApplicationCore.Shared;
using ApplicationCore.Shared.Services;
using CADCodeProxy.CSV;
using CADCodeProxy.Machining;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Outlook;
using MimeKit;
using QuestPDF.Fluent;
using System.Runtime.InteropServices;
using UglyToad.PdfPig.Writer;
using System.Diagnostics;
using Domain.Excel;
using static ApplicationCore.Features.MDFDoorOrders.DoorOrderRelease.NamedPipeServer;
using Action = System.Action;
using OutlookApp = Microsoft.Office.Interop.Outlook.Application;
using ExcelApp = Microsoft.Office.Interop.Excel.Application;
using Range = Microsoft.Office.Interop.Excel.Range;
using Domain.Components.ProgressModal;
using Domain.Services;
using Domain.Extensions;
using OrderExporting.CNC.Programs;
using OrderExporting.CNC.ReleasePDF;
using OrderExporting.CNC.Programs.WSXML;
using OrderExporting.CNC.Programs.Job;
using OrderExporting.CNC.Programs.WorkOrderReleaseEmail;
using OrderExporting.CNC.Programs.WSXML.Report;
using ApplicationCore.Features.MDFDoorOrders.DoorOrderRelease.OrderTracker;

namespace ApplicationCore.Features.MDFDoorOrders.DoorOrderRelease;

public class DoorOrderReleaseActionRunner : IActionRunner {

    public Action? ShowProgressBar { get; set; }
    public Action? HideProgressBar { get; set; }
    public Action<int>? SetProgressBarValue { get; set; }
    public Action<ProgressLogMessage>? PublishProgressMessage { get; set; }

    public DoorOrder? DoorOrder { get; set; }
    public DoorOrderReleaseOptions? Options { get; set; }

    private readonly CNCPartGCodeGenerator _generator;
    private readonly CNCReleaseDecoratorFactory _releaseDecoratorFactory;
    private readonly IFileReader _fileReader;
    private readonly IEmailService _emailService;
    private readonly IWSXMLParser _wsxmlParser;
    private readonly IWindowFocuser _windowFocuser;

    public DoorOrderReleaseActionRunner(CNCPartGCodeGenerator generator, CNCReleaseDecoratorFactory releaseDecoratorFactory, IFileReader fileReader, IEmailService emailService, IWSXMLParser wsxmlParser, IWindowFocuser windowFocuser) {
        _generator = generator;
        _releaseDecoratorFactory = releaseDecoratorFactory;
        _fileReader = fileReader;

        _generator.ShowProgressBar += () => ShowProgressBar?.Invoke();
        _generator.HideProgressBar += () => HideProgressBar?.Invoke();
        _generator.OnProgressReport += (msg) => PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, msg));
        _generator.OnError += (msg) => PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, msg)); ;
        _generator.SetProgressBarValue += (prog) => SetProgressBarValue?.Invoke(prog);
        _emailService = emailService;
        _wsxmlParser = wsxmlParser;
        _windowFocuser = windowFocuser;
    }

    public async Task Run() {

        if (DoorOrder is null || Options is null) {
            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "Door order or release options not set"));
            return;
        }

        await ReleaseDoorOrder(_generator, DoorOrder, Options);

    }

    public async Task ReleaseDoorOrder(CNCPartGCodeGenerator generator, DoorOrder doorOrder, DoorOrderReleaseOptions options) {

        if (!File.Exists(doorOrder.OrderFile) || Path.GetExtension(doorOrder.OrderFile) != ".xlsm") {
            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "Invalid door order file"));
            return;
        }

        if (!Directory.Exists(options.OutputDirectory)) {
            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "Output directory does not exist"));
            return;
        }

        string? workbookPdfTmpFilePath = null;
        List<Document> documents = [];
        List<ReleasedJob> releasedJobs = [];

        var (generatedGCodeDocument, tmpPdf, generatedJobs) = await GetReleasePDFAndGenerateGCode(generator, doorOrder, options);

        if (options.GenerateGCodeFromWorkbook && generatedGCodeDocument is null) {
            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "Release failed."));
            return;
        }

        if (generatedGCodeDocument is not null) {
            documents.Add(generatedGCodeDocument);
        }

        if (tmpPdf is not null) {
            workbookPdfTmpFilePath = tmpPdf;
        }

        if (generatedJobs is not null) {
            releasedJobs.AddRange(generatedJobs);
        }

        if (options.AddExistingCSVTokens) {

            var (csvDocument, csvJobs) = await GenerateGCodeFromCSVFile(generator, doorOrder, options.CSVTokenFilePath);

            if (csvDocument is null) {
                PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "Release failed."));
                return;
            } else {
                documents.Add(csvDocument);
            }

            releasedJobs.AddRange(csvJobs);

        }

        if (options.AddExistingWSXMLReport) {

            var job = await GetWSXMLReleasedJobs(options.WSXMLReportFilePath, DateTime.Today, DateTime.Today, doorOrder.Customer, doorOrder.Vendor);
            if (job is not null) {

                var decorator = _releaseDecoratorFactory.Create(job);
                var doc = Document.Create(decorator.Decorate);
                documents.Add(doc);

                releasedJobs.Add(job);

            } else {
                PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "Release failed."));
                return;
            }

        }

        if (options.PrintFile) {
            foreach (var doc in documents) {
                await PrintDocument(doc);
            }
        }

        string? mergedFilePath = await MergeReleasePDF(options, workbookPdfTmpFilePath, [.. documents]);

        if (mergedFilePath is null || !File.Exists(mergedFilePath)) {
            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "No file was generated"));
            return;
        } else {

            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.FileCreated, mergedFilePath));

        }

        if (workbookPdfTmpFilePath is not null && _fileReader.DoesFileExist(workbookPdfTmpFilePath)) {
            File.Delete(workbookPdfTmpFilePath);
        }

        if (options.SendEmail) {

            string note = ""; // TODO: Read note from workbook
            List<string> attachments = [];
            if (mergedFilePath is not null) {
                attachments.Add(mergedFilePath);
            }

            var (HTMLBody, TextBody) = GenerateEmailBodies(true, releasedJobs, note);

            if (options.PreviewEmail) {
                await Task.Run(() => CreateAndDisplayOutlookEmail(options.EmailRecipients, $"RELEASED: {doorOrder.OrderNumber} {doorOrder.Customer}", HTMLBody, TextBody, attachments));
            } else {
                await SendEmailAsync(options.EmailRecipients, $"RELEASED: {doorOrder.OrderNumber} {doorOrder.Customer}", HTMLBody, TextBody, attachments);
            }
        }

        if (options.PostToTracker) {
            try {
                await PostOrderToTracker(doorOrder);
            } catch (System.Exception ex) {
                PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, $"Failed to post order to online tracker. - {ex.Message}"));
            }
        }

    }

    private async Task PrintDocument(Document document) {

        var data = document.GeneratePdf();
        var file = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pdf");

        await File.WriteAllBytesAsync(file, data);

        var proc = new Process() {
            StartInfo = new ProcessStartInfo() {
                CreateNoWindow = true,
                Verb = "print",
                UseShellExecute = true,
                FileName = file
            }
        };

        if (proc.Start()) {
            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, "Printing CNC cutlist"));
             _ = proc.WaitForExitAsync().ContinueWith(t => File.Delete(file));
        }

    }

    private async Task<(Document?, string?, List<ReleasedJob>)> GetReleasePDFAndGenerateGCode(CNCPartGCodeGenerator generator, DoorOrder doorOrder, DoorOrderReleaseOptions options) {

        string? workbookPdfTmpFilePath = null;
        Document? generatedGCodeDocument = null;
        List<ReleasedJob> releasedJobs = [];

        DateTime orderDate = DateTime.Today;
        DateTime dueDate = DateTime.Today;

        Batch[] batches = [];
        bool wasOrderOpen = true;
        await Task.Run(() => {

            var app = GetExcelInstance();

            try {

                if (app is null) {
                    wasOrderOpen = false;
                    return;
                }

                app.ScreenUpdating = false;
                app.DisplayAlerts = false;

                var workbooks = app.Workbooks;
                var workbook = workbooks.Open(doorOrder.OrderFile);
                var worksheets = workbook.Worksheets;

                if (options.GenerateGCodeFromWorkbook) {
                    batches = GenerateBatchesFromDoorOrder(app, worksheets, doorOrder);

                    if (batches.Length == 0) {
                        PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "No CNC batches where generated from workbook"));
                    }

                }

                if (options.IncludeCover || options.IncludePackingList || options.IncludeInvoice || options.IncludeOrderForm) {
                    workbookPdfTmpFilePath = GeneratePDFFromWorkbook(workbook, worksheets, options.IncludeCover, options.IncludePackingList, options.IncludeInvoice, options.IncludeOrderForm, options.PrintFile);
                }

                UpdateReleaseDateOnWorkbook(worksheets);

                orderDate = ReadDateTimeFromWorkbook(worksheets, "MDF", "OrderDate");
                dueDate = ReadDateTimeFromWorkbook(worksheets, "MDF", "DueDate");

            } finally {

                if (app is not null) {
                    app.ScreenUpdating = true;
                    app.DisplayAlerts = true;
                }

                _windowFocuser.TryToSetMainWindowFocus();

            }

        });

        if (!wasOrderOpen) {
            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "Door order is not open"));
        }

        if (batches.Length != 0) {
            var (document, jobs) = await CreateCutListDocumentForBatches(generator, doorOrder, batches, orderDate, dueDate);
            generatedGCodeDocument = document;
            releasedJobs.AddRange(jobs);
        }

        return (generatedGCodeDocument, workbookPdfTmpFilePath, releasedJobs);

    }

    private async Task<(Document?, List<ReleasedJob>)> GenerateGCodeFromCSVFile(CNCPartGCodeGenerator generator, DoorOrder doorOrder, string tokenFile) {

        var fileInfo = new FileInfo(tokenFile);
        if (!fileInfo.Exists) {
            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "CSV token file does not exist."));
            return (null, []);
        }

        PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, $"Reading CSV Token File - {tokenFile}"));

        try {

            var batches = new CSVTokenReader().ReadBatchCSV(tokenFile);

            if (batches.Length == 0) {
                PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "No CNC batches where generated from workbook"));
            }

            DateTime orderDate = DateTime.Now;
            DateTime dueDate = DateTime.Now;

            return await CreateCutListDocumentForBatches(generator, doorOrder, batches, orderDate, dueDate);

        } catch (System.Exception ex) {

            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, $"Failed to read CSV tokens - {ex.Message}"));
            return (null, []);

        }

    }

    private async Task<string?> MergeReleasePDF(DoorOrderReleaseOptions options, string? tmpReleasePDFFilePath, Document[] documents) {

        var fileComponents = new List<byte[]>();

        if (tmpReleasePDFFilePath is not null && _fileReader.DoesFileExist(tmpReleasePDFFilePath)) {
            var mdfReleasePagesData = await File.ReadAllBytesAsync(tmpReleasePDFFilePath);
            fileComponents.Add(mdfReleasePagesData);
        }

        var mergedDocument = await Task.Run(() => {

            documents.ForEach(document => {
                var pdfData = document.GeneratePdf();
                fileComponents.Add(pdfData);
            });

            if (fileComponents.Count == 0) {
                return [];
            }

            return PdfMerger.Merge(fileComponents);

        });

        if (fileComponents.Count == 0) {
            return null;
        }

        var mergedFilePath = _fileReader.GetAvailableFileName(options.OutputDirectory, options.FileName, ".pdf");
        await File.WriteAllBytesAsync(mergedFilePath, mergedDocument);

        return mergedFilePath;

    }

    private Batch[] GenerateBatchesFromDoorOrder(ExcelApp app, Sheets worksheets, DoorOrder doorOrder) {

        PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, "Generating CSV Token File"));
        var fileName = Path.GetFileName(doorOrder.OrderFile);

        string tokenFile = GetTokenFilePath(doorOrder, app, worksheets, fileName);

        var server = new NamedPipeServer();
        server.MessageReceived += ProcessMessage;
        var serverTask = Task.Run(server.Start);

        bool wasSuccessful = false;
        DateTime startTimestamp = DateTime.UtcNow;

        ShowProgressBar?.Invoke();
        var macroTask = Task.Run(() => {
            try {
                ExecuteDoorProcessorMacro(app, fileName);
                wasSuccessful = true;
            } catch (System.Exception ex) {
                PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, $"An error ocurred while trying to generate CSV token file. - {ex.Message}"));
            }
        });
        macroTask.Wait();
        HideProgressBar?.Invoke();

        server.Stop();

        var fileInfo = new FileInfo(tokenFile);
        if (!wasSuccessful || !fileInfo.Exists || fileInfo.LastWriteTimeUtc < startTimestamp) {
            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "CSV token file was not generated."));
            return [];
        }

        PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, $"Reading CSV Token File - {tokenFile}"));

        try {

            var batches = new CSVTokenReader().ReadBatchCSV(tokenFile);
            return batches;

        } catch (System.Exception ex) {

            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, $"Failed to read CSV tokens - {ex.Message}"));
            return [];

        }

    }

    private static void ExecuteDoorProcessorMacro(ExcelApp app, string fileName) {
        app.GetType()
            .InvokeMember("Run",
                          System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.InvokeMethod,
                          null,
                          app,
                          new object[] { $"'{fileName}'!SilentDoorProcessing" });
    }

    private static string GetTokenFilePath(DoorOrder doorOrder, ExcelApp app, Sheets worksheets, string fileName) {

        try {

            var result = app.GetType()
                            .InvokeMember("Run",
                                          System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.InvokeMethod,
                                          null,
                                          app,
                                          new object[] { $"'{fileName}'!GetExportFilePath" });

            if (result is string filePath) {
                return filePath;
            }

        } catch {
            // Could not get export file path from workbook
        }

        string exportDirectory = string.Empty;
        if (TryGetSheet(worksheets, "MDF Door Data", out Worksheet dataSheet)) {
             exportDirectory = dataSheet.Range["ExportFile"].Value2;
        }

        return Path.Combine(exportDirectory, $"{doorOrder.OrderNumber} - DoorTokens.csv");

    }

    private async Task<(Document?, List<ReleasedJob>)> CreateCutListDocumentForBatches(CNCPartGCodeGenerator generator, DoorOrder doorOrder, Batch[] batches, DateTime orderDate, DateTime dueDate) {

        List<ReleasedJob> releasedJobs = [];

        List<ICNCReleaseDecorator> decorators = [];

        PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, "Generating GCode For Doors"));

        try {

            foreach (var batch in batches) {

                var job = await generator.GenerateGCode(batch, doorOrder.Customer, doorOrder.Vendor, orderDate, dueDate);

                if (job is null) {
                    continue;
                }

                var decorator = _releaseDecoratorFactory.Create(job);
                if (decorator is not null) {
                    decorators.Add(decorator);
                }

                releasedJobs.Add(job);

            }

        } catch (System.Exception ex) {

            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, $"Error generating gcode for batch - {ex.Message}"));

        }

        PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, "Creating CNC Release Document"));

        if (decorators.Count == 0) {
            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "No released jobs where created"));
            return (null, []);
        }

        try {

            var document = await Task.Run(() => {
                return Document.Create(doc => {

                    foreach (var decorator in decorators) {
                        decorator.Decorate(doc);
                    }

                });
            });

            return (document, releasedJobs);

        } catch {

            return (null, []);

        }

    }

    private async Task<ReleasedJob?> GetWSXMLReleasedJobs(string wsxmlFile, DateTime orderDate, DateTime? dueDate, string customerName, string vendorName) {

        if (Path.GetExtension(wsxmlFile) != ".xml") {
            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "CADCode report file is an invalid file type"));
            return null;
        }

        ReleasedJob? jobData = null;
        WSXMLReport? report = await Task.Run(() => WSXMLParser.ParseWSXMLReport(wsxmlFile));
        if (report is not null) {
            jobData = _wsxmlParser.MapDataToReleasedJob(report, orderDate, dueDate, customerName, vendorName);
        }

        PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, $"WSXML job loaded"));

        return jobData;
    }

    private static string? GeneratePDFFromWorkbook(Workbook workbook, Sheets worksheets, bool cover, bool packingList, bool invoice, bool orderForm, bool print) {

        var PDFSheetNames = new List<string>();
        if (cover) {
            const string sheetName = "MDF Cover Sheet";
            SetPrintArea(worksheets, sheetName, "J");
            PDFSheetNames.Add(sheetName);

            if (print && TryGetSheet(worksheets, sheetName, out Worksheet sheet)) {
                sheet.PrintOutEx();
            }
        }
        if (packingList) {
            const string sheetName = "MDF Packing List";
            SetPrintArea(worksheets, sheetName, "E");
            PDFSheetNames.Add(sheetName);

            if (print && TryGetSheet(worksheets, sheetName, out Worksheet sheet)) {
                sheet.PrintOutEx(Copies: 2);
            }
        }
        if (invoice) {
            const string sheetName = "MDF Invoice";
            SetPrintArea(worksheets, sheetName, "E");
            PDFSheetNames.Add(sheetName);

            if (print && TryGetSheet(worksheets, sheetName, out Worksheet sheet)) {
                sheet.PrintOutEx();
            }
        }
        if (orderForm) {
            const string sheetName = "MDF Order Form";
            if (TryGetSheet(worksheets, sheetName, out Worksheet sheet)) {
                if (print) {
                    sheet.PageSetup.PrintArea = "A1:G86";
                    sheet.PrintOutEx(Copies: 2);
                }
                sheet.PageSetup.PrintArea = "A1:G45";
            }
            PDFSheetNames.Add(sheetName);
        }

        if (PDFSheetNames.Count == 0) return null;

        Worksheet previouslyActiveSheet = workbook.ActiveSheet;

        var tmpFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".pdf";

        var sheetsToSelect = GetSheetNamesToSelect(worksheets, PDFSheetNames);
        worksheets[sheetsToSelect].Select();

        Worksheet activeSheet = workbook.ActiveSheet;
        activeSheet.ExportAsFixedFormat2(XlFixedFormatType.xlTypePDF, tmpFileName, OpenAfterPublish: false);

        previouslyActiveSheet?.Select();

        return tmpFileName;

    }

    private static string[] GetSheetNamesToSelect(Sheets worksheets, List<string> PDFSheetNames) {
        List<string> sheetsToSelect = [];
        var enumerator = worksheets.GetEnumerator();
        while (enumerator.MoveNext()) {
            string? foundItem = null;
            foreach (var sheetName in PDFSheetNames) {
                if (((Worksheet)enumerator.Current).Name.Equals(sheetName)) {
                    foundItem = sheetName;
                    sheetsToSelect.Add(sheetName);
                    break;
                }
            }
            if (foundItem is not null) PDFSheetNames.Remove(foundItem);
        }
        return sheetsToSelect.ToArray();
    }

    private static void SetPrintArea(Sheets worksheets, string sheetName, string lastCol) {

        if (!TryGetSheet(worksheets, sheetName, out Worksheet worksheet)) {
            return;
        }

        const int maxRow = 210;
        int lastRow = 1;

        for (int currentRow = 1; currentRow <= maxRow; currentRow++) {

            Range rng = worksheet.Range[$"A{currentRow}"];
            var val = rng.Value2?.ToString() ?? "";
            if (!string.IsNullOrWhiteSpace(val) && val != "-") {
                lastRow = currentRow;
            }

        }

        worksheet.PageSetup.PrintArea = $"A1:{lastCol}{lastRow}";

    }

    private static bool TryGetSheet(Sheets worksheets, object index, out Worksheet sheet) {

        try {

            sheet = worksheets[index];
            return true;

        } catch {

            sheet = null;
            return false;

        }

    }

    private static void UpdateReleaseDateOnWorkbook(Sheets worksheets) {

        if (!TryGetSheet(worksheets, "MDF", out Worksheet orderSheet)) {
            return;
        }

        try {

        var rng = orderSheet.Range["ReleasedDate"];
        rng.Value2 = DateTime.Today.ToShortDateString();

        } catch {

        }

    }

    private static DateTime ReadDateTimeFromWorkbook(Sheets worksheets, string sheetName, string rangeName) {

        try {

            if (!TryGetSheet(worksheets, sheetName, out Worksheet sheet)) {
                return DateTime.Today;
            }

            var rng = sheet.Range[rangeName];

            if (rng.Value2 is double oaDate) {
                return DateTime.FromOADate(oaDate);
            }

            return DateTime.Today;

        } catch {

            return DateTime.Today;

        }

    }

    private void CreateAndDisplayOutlookEmail(string recipients, string subject, string htmlBody, string textBody, IEnumerable<string> attachments) {

        var app = new OutlookApp();
        MailItem mailItem = (MailItem)app.CreateItem(OlItemType.olMailItem);
        mailItem.To = recipients;
        mailItem.Subject = subject;
        mailItem.Body = textBody;
        mailItem.HTMLBody = htmlBody;

        attachments.Where(_fileReader.DoesFileExist).ForEach(att => mailItem.Attachments.Add(att));

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

    private async Task SendEmailAsync(string recipients, string subject, string htmlBody, string textBody, IEnumerable<string> attachments) {

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

    private static (string HTMLBody, string TextBody) GenerateEmailBodies(bool includeReleaseSummary, List<ReleasedJob> jobs, string note) {

        var releasedJobs = jobs.Where(j => j.Releases.Any())
                                .Select(job => {

                                    var usedMaterials = job.Releases.First().GetUsedMaterials();
                                    var usedEdgeBanding = job.Releases.First().GetUsedEdgeBanding();

                                    return new Job(job.JobName, usedMaterials, usedEdgeBanding);

                                });

        var model = new ReleasedWorkOrderSummary(releasedJobs, false, false, false, note);

        var htmlBody = ReleaseEmailBodyGenerator.GenerateHTMLReleaseEmailBody(model, includeReleaseSummary);
        var textBody = ReleaseEmailBodyGenerator.GenerateTextReleaseEmailBody(model, includeReleaseSummary);

        return (htmlBody, textBody);

    }

    private void ProcessMessage(PipeMessage message) {

        switch (message.Type) {
            case "info":
                PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, $"{message.MessageA} - {message.MessageB}"));
                break;
            case "warning":
                PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, $"WARNING {message.MessageA} - {message.MessageB}"));
                break;
            case "error":
                PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, $"{message.MessageA} - {message.MessageB}"));
                break;
            case "progress":
                PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, $"{message.MessageA}"));
                if (int.TryParse(message.MessageB, out int percentComplete)) {
                    SetProgressBarValue?.Invoke(percentComplete);
                }
                break;
            default:
                PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, $"{message.Type}|{message.MessageA}|{message.MessageB}"));
                break;
        }

    }

    private ExcelApp? GetExcelInstance() {

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

    private  async Task PostOrderToTracker(DoorOrder doorOrder) {

        var client = new OrderTrackerApiClient();

        DateTime orderedDate = DateTime.Today;
        if (doorOrder.OrderDate is DateTime date) {
            orderedDate = date;
        }

        DateTime dueDate = DateTime.Today.AddDays(14);
        if (doorOrder.DueDate is DateTime date2) {
            dueDate = date2;
        }

        var order = new NewOrder() {
            Number = doorOrder.OrderNumber,
            Name = doorOrder.OrderName,
            Customer = doorOrder.Customer,
            Vendor = doorOrder.Vendor,
            OrderedDate = orderedDate,
            WantByDate = dueDate,
            IsRush = false,
            Price = 0,
            Shipping = 0,
            Tax = 0,
            Note = ""
        };

        var createdOrder = await client.PostNewOrder(order);

        if (createdOrder is null) {
            PublishProgressMessage?.Invoke(new ProgressLogMessage(ProgressLogMessageType.Error, "Failed to post order to tracker"));
            return;
        }

        var release = new NewMDFDoorRelease() {
            ItemCount = doorOrder.ItemCount
        };

        var createdRelease = await client.PostNewMDFDoorRelease(createdOrder.Id, release);

        if (createdRelease is null) {
            PublishProgressMessage?.Invoke(new ProgressLogMessage(ProgressLogMessageType.Error, "Failed to post mdf door release to tracker"));
            return;
        }

    }

}
