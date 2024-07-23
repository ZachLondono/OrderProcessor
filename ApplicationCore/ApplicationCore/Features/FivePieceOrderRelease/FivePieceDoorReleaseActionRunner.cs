using ApplicationCore.Shared.Services;
using Domain.Components.ProgressModal;
using Action = System.Action;

namespace ApplicationCore.Features.FivePieceOrderRelease;

public class FivePieceDoorReleaseActionRunner(FivePieceDoorReleasePDFGenerator pdfGenerator, IEmailService emailService) : IActionRunner {

    public Action? ShowProgressBar { get; set; }
    public Action? HideProgressBar { get; set; }
    public Action<int>? SetProgressBarValue { get; set; }
    public Action<ProgressLogMessage>? PublishProgressMessage { get; set; }

    public FivePieceOrder? Order { get; set; }
    public FivePieceOrderReleaseOptions? Options { get; set; }

    private readonly FivePieceDoorReleasePDFGenerator _pdfGenerator = pdfGenerator;
    private readonly IEmailService _emailService = emailService;

    public async Task Run() {

        if (Order is null) {
            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "No order data"));
            return;
        }

        if (Options is null) {
            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "No options set"));
            return;
        }

        if (!Directory.Exists(Options.OutputDirectory)) {
            PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, "Output directory could not be found"));
            return;
        }

        var filePath = _pdfGenerator.ReleaseOrder(Order, Options.OutputDirectory);

        PublishProgressMessage?.Invoke(new(ProgressLogMessageType.FileCreated, filePath));

        if (Options.SendEmail) {
            await SendEmail(Options.EmailRecipients,
                            Order.TrackingNumber,
                            Order.CompanyName,
                            Options.PreviewEmail,
                            _emailService,
                            filePath);
        }

        PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Success, "Done"));

    }

    private static async Task SendEmail(string recipients, string trackingNumber, string customer, bool previewEmail, IEmailService emailService, string filePath) {
        var sender = new EmailSender() {
            Recipients = recipients,
            Subject = $"RELEASED: {trackingNumber} {customer}",
            Attachments = [filePath],
            Body = "Please see attached release.",
            EmailService = emailService
        };

        if (previewEmail) {
            await Task.Run(sender.PreviewEmail);
        } else {
            await sender.SendEmailAsync();
        }
    }
}