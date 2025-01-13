using Domain.Extensions;
using ApplicationCore.Shared.Services;
using Domain.Services;
using MimeKit;
using QuestPDF.Fluent;
using OrderExporting.CNC.ReleasePDF;
using OrderExporting.CNC.Programs.WSXML;
using OrderExporting.CNC.Programs.WorkOrderReleaseEmail;
using OrderExporting.CNC.Programs.Job;
using OrderExporting.CNC.Programs.WSXML.Report;
using UglyToad.PdfPig.Writer;
using Microsoft.Extensions.Logging;
using ApplicationCore.Shared.Settings;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace ApplicationCore.Features.GeneralReleasePDF;

internal class ReleasePDFDialogViewModel {

    public Action? OnPropertyChanged { get; set; }

    public Model Model { get; set; } = new();

    private string? _error = null;
    public string? Error {
        get => _error;
        set {
            _error = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private bool _isGeneratingPDF = false;
    public bool IsGeneratingPDF {
        get => _isGeneratingPDF;
        set {
            _isGeneratingPDF = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private List<string> _generatedFile = new();
    public List<string> GeneratedFiles {
        get => _generatedFile;
        set {
            _generatedFile = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public CNCReleaseSettings Settings { get; init; }

    private readonly ILogger<ReleasePDFDialogViewModel> _logger;
    private readonly ICNCReleaseDecorator _cncReleaseDecorator;
    private readonly IFileReader _fileReader;
    private readonly IEmailService _emailService;
    private readonly IWSXMLParser _wsxmlParser;

    public ReleasePDFDialogViewModel(ILogger<ReleasePDFDialogViewModel> logger, ICNCReleaseDecorator cncReleaseDecorator, IFileReader fileReader, IEmailService emailService, IWSXMLParser wsxmlParser, IOptions<CNCReleaseSettings> settings) {
        _logger = logger;
        _cncReleaseDecorator = cncReleaseDecorator;
        _fileReader = fileReader;
        _emailService = emailService;
        _wsxmlParser = wsxmlParser;
        Settings = settings.Value;
    }

    public void InitializeModel() {

        Model.CustomerName = string.Empty;
        Model.VendorName = string.Empty;
        Model.FileName = string.Empty;
        Model.OutputDirectory = Settings.DefaultOutputDirectory;
        Model.SendEmail = Settings.SendEmail;
        Model.EmailRecipients = string.Empty; 
        Model.Print = Settings.Print;

        FindMostRecentReport();

        GeneratedFiles = [];

    }

    public void FindMostRecentReport() {

        var directory = Settings.WSXMLReportsDirectory;
        var dirInfo = new DirectoryInfo(directory);

        if (!dirInfo.Exists) {
            // If the reports directory does not exist, don't attempt to find a report
            return;
        }

        Model.ReportFilePath = dirInfo
                                    .GetFiles()
                                    .OrderByDescending(f => f.LastWriteTime)
                                    .FirstOrDefault(f => f.Extension == ".xml")
                                    ?.FullName ?? string.Empty;

    }

    public async Task GeneratePDF() {

        Error = null;

        if (string.IsNullOrEmpty(Model.ReportFilePath)) {
            Error = "WSXML report is required";
            return;
        }

        if (Path.GetExtension(Model.ReportFilePath) != ".xml") {
            Error = "Invalid WSXML file";
            return;
        }

        if (!File.Exists(Model.ReportFilePath)) {
            Error = "Report file not found";
            return;
        }

        if (string.IsNullOrWhiteSpace(Model.OutputDirectory)) {
            Error = "Output directory required";
            return;
        }

        string[] outputDirectories = Model.OutputDirectory.Split(';');

        foreach (var directory in outputDirectories) {
            if (!Directory.Exists(directory)) {
                Error = $"Output directory does not exist or cannot be accessed - '{directory}'";
                return;
            }
        }

        IsGeneratingPDF = true;

        try {

            ReleasedJob? job = null;

            await Task.Run(() => {
                WSXMLReport? report = WSXMLParser.ParseWSXMLReport(Model.ReportFilePath);
                if (report is not null) {
                    job = _wsxmlParser.MapDataToReleasedJob(report, Model.OrderDate, Model.DueDate, Model.CustomerName, Model.VendorName);
                }
            });

            if (job is null) {

                Error = "No data read from file";
                GeneratedFiles = [];
                _logger.LogError("No job data was read from WSXML report {WSXMLFilePath}", Model.ReportFilePath);
                IsGeneratingPDF = false;
                return;

            }

            await Task.Run(() => {

                _cncReleaseDecorator.AddData(job);

                byte[] fileData = [];

                var doc = Document.Create(_cncReleaseDecorator.Decorate);
                fileData = doc.GeneratePdf();

                if (Model.AdditionalFilePaths.Count != 0) {

                    List<byte[]> components = [fileData];

                    foreach (var path in Model.AdditionalFilePaths) {
                        var bytes = File.ReadAllBytes(path);
                        components.Add(bytes);
                    }

                    fileData = PdfMerger.Merge(components);

                }

                foreach (var directory in outputDirectories) {
                    var outputFilePath = _fileReader.GetAvailableFileName(directory, Model.FileName, "pdf");
                    File.WriteAllBytes(outputFilePath, fileData);
                    GeneratedFiles.Add(outputFilePath);
                }

            });

            if (Model.SendEmail && !string.IsNullOrWhiteSpace(Model.EmailRecipients) && GeneratedFiles.Count != 0) {
                try {
                    await SendReleaseEmail(job, GeneratedFiles.First(), Model.EmailRecipients);
                } catch (Exception ex) {
                    Error = "Failed to send release email";
                    _logger.LogError(ex, "Exception thrown while attempting to send release email");
                }
            }

            if (Model.Print && GeneratedFiles.Count != 0) {
                try {
                    Print(GeneratedFiles.First());
                } catch (Exception ex) {
                    Error = "Failed to print pdf";
                    _logger.LogError(ex, "Exception thrown while attempting to print cnc release pdf");
                }
            }

        } catch (Exception ex) {
            Error = "Failed to generate pdf";
            GeneratedFiles = [];
            _logger.LogError(ex, "Exception thrown while attempting to generate cnc release pdf");
        }

        IsGeneratingPDF = false;

    }

    private async Task SendReleaseEmail(ReleasedJob job, string filePath, string recipients) {
        var message = new MimeMessage();
        recipients.Split(';')
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ForEach(r => message.To.Add(new MailboxAddress(r, r)));
        var sender = _emailService.GetSender();
        message.From.Add(sender);
        message.Subject = $"RELEASED: {job.JobName} - {job.CustomerName}";

        var usedMaterials = job.Releases.First().GetUsedMaterials();
        var usedEdgeBanding = job.Releases.First().GetUsedEdgeBanding();

        IEnumerable<Job> jobs = [
            new(job.JobName, usedMaterials, usedEdgeBanding)
        ];
        var model = new ReleasedWorkOrderSummary(jobs, false, false, false, null);

        var body = ReleaseEmailBodyGenerator.GenerateHTMLReleaseEmailBody(model, true);
        var builder = new BodyBuilder {
            TextBody = body,
            HtmlBody = body
        };
        builder.Attachments.Add(filePath);
        message.Body = builder.ToMessageBody();
        await _emailService.SendMessageAsync(message);
    }

    private static void Print(string filePath) {

        var proc = new Process() {
            StartInfo = new ProcessStartInfo() {
                CreateNoWindow = true,
                Verb = "print",
                UseShellExecute = true,
                FileName = filePath
            }
        };

        proc.Start();

    }

}
