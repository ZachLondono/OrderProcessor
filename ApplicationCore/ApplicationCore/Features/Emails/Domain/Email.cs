namespace ApplicationCore.Features.Emails.Domain;

public class Email {

    public EmailSender Sender { get; set; }

    private readonly List<string> _recipients;
    public IReadOnlyList<string> Recipients => _recipients;

    public string Subject { get; set; }

    public string Body { get; set; }

    public Email(EmailSender sender, IEnumerable<string> recipients, string subject, string body) {
        Sender = sender;
        _recipients = new(recipients);
        Subject = subject;
        Body = body;
    }

    public void AddRecipient(string recipient) {
        _recipients.Add(recipient);
    }

    // TODO: add attachments

}
