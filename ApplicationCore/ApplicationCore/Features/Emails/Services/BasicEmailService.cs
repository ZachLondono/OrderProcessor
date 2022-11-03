using ApplicationCore.Features.Emails.Domain;
using MimeKit;

namespace ApplicationCore.Features.Emails.Services;

internal class BasicEmailService : IEmailService {

    private readonly ISmtpClientFactory _clientFactory;

    public BasicEmailService(ISmtpClientFactory clientFactory) {
        _clientFactory = clientFactory;
    }

    public async Task<string> SendEmailAsync(Email email) {

        var body = new BodyBuilder {
            TextBody = email.Body
        };

        var multiPart = new Multipart("mixed") {
            body.ToMessageBody()
        };

        foreach (var attachment in email.Attachments) {
            multiPart.Add(new MimePart() {
                Content = new MimeContent(File.OpenRead(attachment)),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = Path.GetFileName(attachment)
            });
        }

        var message = new MimeMessage {
            Sender = MailboxAddress.Parse(email.Sender.Email),
            Subject = email.Subject,
            Body = multiPart
        };

        message.To.AddRange(
                        email.Recipients.Select(addr => MailboxAddress.Parse(addr))
                    );

        using var smtp = _clientFactory.CreateClient();
        await smtp.ConnectAsync(email.Sender.Host, email.Sender.Port);
        await smtp.AuthenticateAsync(email.Sender.Email, email.Sender.Password);
        var response = await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);

        return response;

    }

}
