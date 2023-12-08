using ApplicationCore.Features.OpenDoorOrders;
using ApplicationCore.Features.Orders.OrderRelease.Handlers.CNC;
using ApplicationCore.Shared;
using ApplicationCore.Shared.CNC;
using ApplicationCore.Shared.CNC.Job;
using ApplicationCore.Shared.CNC.ReleasePDF;
using ApplicationCore.Shared.CNC.WorkOrderReleaseEmail;
using ApplicationCore.Shared.CNC.WSXML.Report;
using ApplicationCore.Shared.CNC.WSXML;
using ApplicationCore.Shared.Components.ProgressModal;
using ApplicationCore.Shared.Services;
using CADCodeProxy.CSV;
using CADCodeProxy.Machining;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Outlook;
using MimeKit;
using QuestPDF.Fluent;
using System.Runtime.InteropServices;
using UglyToad.PdfPig.Writer;
using static ApplicationCore.Layouts.MainLayout.DoorOrderRelease.NamedPipeServer;
using Action = System.Action;
using OutlookApp = Microsoft.Office.Interop.Outlook.Application;
using ExcelApp = Microsoft.Office.Interop.Excel.Application; 
using System.Diagnostics;

namespace ApplicationCore.Layouts.MainLayout.DoorOrderRelease;

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

        var (generatedGCodeDocument, tmpPdf, jobs) = await GetReleasePDFAndGenerateGCode(generator, doorOrder, options);

        if (generatedGCodeDocument is not null) {
            documents.Add(generatedGCodeDocument);
        }

        if (tmpPdf is not null) {
            workbookPdfTmpFilePath = tmpPdf;
        }

        if (jobs is not null) {
            releasedJobs.AddRange(jobs);
        }

        if (options.AddExistingWSXMLReport) {

            var job = await GetWSXMLReleasedJobs(options.WSXMLReportFilePath, DateTime.Today, DateTime.Today, doorOrder.Customer, doorOrder.Vendor);
            if (job is not null) {

                var decorator = _releaseDecoratorFactory.Create(job);
                var doc = Document.Create(decorator.Decorate);
                documents.Add(doc);

                releasedJobs.Add(job);

            }

        }
    
        string? mergedFilePath = await MergeReleasePDF(options, workbookPdfTmpFilePath, [.. documents]);

        if (mergedFilePath is null || !File.Exists(mergedFilePath)) {
            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "No file was generated"));
        } else {

            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.FileCreated, mergedFilePath));

            if (options.PrintFile) {

                new Process () {
                    StartInfo = new ProcessStartInfo() {
                        CreateNoWindow = true,
                        Verb = "print",
                        UseShellExecute = true,
                        FileName = mergedFilePath 
                    }
                }.Start();

            }

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
    }

    private async Task<(Document?, string?, List<ReleasedJob>)> GetReleasePDFAndGenerateGCode(CNCPartGCodeGenerator generator, DoorOrder doorOrder, DoorOrderReleaseOptions options) {

        string? workbookPdfTmpFilePath = null;
        Document? generatedGCodeDocument = null;
        List<ReleasedJob> releasedJobs = [];

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
                app.Calculation = XlCalculation.xlCalculationManual;

                var workbooks = app.Workbooks;
                var workbook = workbooks.Open(doorOrder.OrderFile);
                var worksheets = workbook.Worksheets;

                if (options.GenerateGCodeFromWorkbook) {
                    batches = GenerateBatchesFromDoorOrder(app, worksheets, doorOrder);
                }

                if (options.IncludeCover || options.IncludePackingList || options.IncludeInvoice) {
                    workbookPdfTmpFilePath = GeneratePDFFromWorkbook(workbook, worksheets, options.IncludeCover, options.IncludePackingList, options.IncludeInvoice);
                }

                UpdateReleaseDateOnWorkbook(worksheets);

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
            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "Door order is not open"));
        }

        if (batches.Length != 0) {
            var (document, jobs) = await CreateCutListDocumentForBatches(generator, doorOrder, batches);
            generatedGCodeDocument = document;
            releasedJobs.AddRange(jobs);
        }

        return (generatedGCodeDocument, workbookPdfTmpFilePath, releasedJobs);

    }

    private async Task<string?> MergeReleasePDF(DoorOrderReleaseOptions options, string? tmpReleasePDFFilePath, Document[] documents) {

        var fileComponents = new List<byte[]>();

        if (tmpReleasePDFFilePath is not null) {
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

        var dataSheet = worksheets["MDF Door Data"];

        var exportDirectory = dataSheet.Range["ExportFile"].Value2;

        var tokenFile = Path.Combine(exportDirectory, $"{doorOrder.OrderNumber} - DoorTokens.csv");

        PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, "Generating CSV Token File"));
        var fileName = Path.GetFileName(doorOrder.OrderFile);

        var server = new NamedPipeServer();
        server.MessageReceived += ProcessMessage;
        var serverTask = Task.Run(server.Start);

        ShowProgressBar?.Invoke();
        var macroTask = Task.Run(() =>
            app.GetType()
                .InvokeMember("Run",
                              System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.InvokeMethod,
                              null,
                              app,
                              new object[] { $"'{fileName}'!SilentDoorProcessing" })
        );
        macroTask.Wait();
        HideProgressBar?.Invoke();

        server.Stop();

        PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, "Reading CSV Token File"));
        var batches = new CSVTokenReader().ReadBatchCSV(tokenFile);

        return batches;

    }

    private async Task<(Document, List<ReleasedJob>)> CreateCutListDocumentForBatches(CNCPartGCodeGenerator generator, DoorOrder doorOrder, Batch[] batches) {

        List<ReleasedJob> releasedJobs = [];

        List<ICNCReleaseDecorator> decorators = [];

        PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, "Generating GCode For Doors"));

        foreach (var batch in batches) {

            var job = await generator.GenerateGCode(batch, doorOrder.Customer, doorOrder.Vendor, DateTime.Today, DateTime.Today);

            if (job is null) {
                continue;
            }

            var decorator = _releaseDecoratorFactory.Create(job);
            decorators.Add(decorator);

            releasedJobs.Add(job);

        }

        PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, "Creating CNC Release Document"));

        var document = await Task.Run(() => {
            return Document.Create(doc => {

                foreach (var decorator in decorators) {
                    decorator.Decorate(doc);
                }

            });
        });

        return (document, releasedJobs);

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

    private static string? GeneratePDFFromWorkbook(Workbook workbook, Sheets worksheets, bool cover, bool packingList, bool invoice) {

        var PDFSheetNames = new List<string>();
        if (cover) PDFSheetNames.Add("MDF Cover Sheet");
        if (packingList) PDFSheetNames.Add("MDF Packing List");
        if (invoice) PDFSheetNames.Add("MDF Invoice");

        if (PDFSheetNames.Count == 0) return null;

        var tmpFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".pdf";

        string[] sheetsToSelect = [.. PDFSheetNames];
        worksheets[sheetsToSelect].Select();
    
		Worksheet activeSheet = workbook.ActiveSheet;
		activeSheet.ExportAsFixedFormat2(XlFixedFormatType.xlTypePDF, tmpFileName, OpenAfterPublish: false);

		var activeSheet = workbook.ActiveSheet;
		activeSheet.ExportAsFixedFormat2(XlFixedFormatType.xlTypePDF, tmpFileName, openAfterPublish: false);

        return tmpFileName;

	}

    private static void UpdateReleaseDateOnWorkbook(Sheets worksheets) {

        var orderSheet = worksheets["MDF"];

        var rng = orderSheet.Range["ReleasedDate"];

        rng.Value2 = DateTime.Today.ToShortDateString();

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

                                    var usedMaterials = job.Releases
                                                            .First()
                                                            .Programs
                                                            .Select(p => p.Material)
                                                            .GroupBy(m => (m.Name, m.Width, m.Length, m.Thickness, m.IsGrained))
                                                            .Select(g => new UsedMaterial(g.Count(), g.Key.Name, g.Key.Width, g.Key.Length, g.Key.Thickness));

                                    return new Job(job.JobName, usedMaterials);

                                });

        var model = new ReleasedWorkOrderSummary(releasedJobs, note);

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

}
