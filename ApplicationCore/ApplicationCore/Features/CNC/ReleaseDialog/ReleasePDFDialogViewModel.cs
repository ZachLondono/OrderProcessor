using ApplicationCore.Features.CNC.Contracts;
using ApplicationCore.Features.CNC.ReleaseEmail;
using ApplicationCore.Shared;
using ApplicationCore.Shared.Services;
using MimeKit;
using QuestPDF.Fluent;

namespace ApplicationCore.Features.CNC.ReleaseDialog;

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

	private readonly ICNCReleaseDecorator _cncReleaseDecorator;
	private readonly IFileReader _fileReader;
	private readonly IEmailService _emailService;
	private readonly ReleaseEmailBodyGenerator _emailBodyGenerator;

	public ReleasePDFDialogViewModel(ICNCReleaseDecorator cncReleaseDecorator, IFileReader fileReader, IEmailService emailService, ReleaseEmailBodyGenerator emailBodyGenerator) {
		_cncReleaseDecorator = cncReleaseDecorator;
		_fileReader = fileReader;
		_emailService = emailService;
		_emailBodyGenerator = emailBodyGenerator;
		Model.OutputDirectory = @"R:\Door Orders\Door Programs";
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
				Error = "Output directory does not exist";
				return;
			}
		}

		IsGeneratingPDF = true;

		try {

			var job = await _cncReleaseDecorator.LoadDataFromFile(Model.ReportFilePath, Model.OrderDate, Model.DueDate, Model.CustomerName, Model.VendorName);

			if (job is not null) {

				var doc = Document.Create(_cncReleaseDecorator.Decorate);

				string filePath = "";
				foreach (var directory in outputDirectories) {
					var outputFilePath = _fileReader.GetAvailableFileName(directory, Model.FileName, "pdf");
					doc.GeneratePdf(outputFilePath);
					GeneratedFiles.Add(outputFilePath);
					filePath = outputFilePath;
				}

				if (Model.SendEmail && !string.IsNullOrWhiteSpace(Model.EmailRecipients)) {
					try {
						await SendReleaseEmail(job, filePath, Model.EmailRecipients);
					} catch {
						Error = "Failed to send release email";
					}
				}

			} else {
				Error = "No data read from file";
				GeneratedFiles = new();
			}

		} catch {
			Error = "Failed to generate pdf";
			GeneratedFiles = new();
		}

		IsGeneratingPDF = false;

	}

	private async Task SendReleaseEmail(ReleasedJob job, string filePath, string recipients) {
        var message = new MimeMessage();
        recipients.Split(';')
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ForEach(r => message.To.Add(new MailboxAddress(r, r)));
        var sender = await _emailService.GetSenderAsync();
        message.From.Add(sender);
        message.Subject = $"RELEASED: {job.JobName} - {job.CustomerName}";

		var usedMaterials = job.Releases
		                        .First()
		                        .Programs
		                        .Select(p => p.Material)
		                        .GroupBy(m => (m.Name, m.Width, m.Length, m.Thickness, m.IsGrained))
		                        .Select(g => new UsedMaterial(g.Count(), g.Key.Name, g.Key.Width, g.Key.Length, g.Key.Thickness));

		IEnumerable<Job> jobs = new Job[] {
			new(job.JobName, usedMaterials)
		};
		var model = new ReleaseEmail.Model(jobs, null);

		var body = _emailBodyGenerator.GenerateHTMLReleaseEmailBody(model, true);
        var builder = new BodyBuilder {
            TextBody = body,
            HtmlBody = body
        };
		builder.Attachments.Add(filePath);
		message.Body = builder.ToMessageBody();
		await _emailService.SendMessageAsync(message);
	}

}
