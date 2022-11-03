namespace ApplicationCore.Features.Emails.Domain;

public class Email {

    public EmailSender Sender { get; set; }

    private readonly List<string> _recipients;
    public IReadOnlyList<string> Recipients => _recipients;

    public string Subject { get; set; }

    public string Body { get; set; }

    public readonly List<string> _attachments;
    public IReadOnlyList<string> Attachments => _attachments;

    public Email(EmailSender sender, IEnumerable<string> recipients, string subject, string body, IEnumerable<string>? attachments = null) {
        Sender = sender;
        _recipients = new(recipients);
        Subject = subject;
        Body = body;
        if (attachments is not null) _attachments = new(attachments);
        else _attachments = new();
    }

    public void AddRecipient(string recipient) {
        _recipients.Add(recipient);
    }

    // TODO: add attachments

}
