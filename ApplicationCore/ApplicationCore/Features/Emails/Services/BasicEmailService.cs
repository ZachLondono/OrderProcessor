using ApplicationCore.Features.Emails.Domain;
using MimeKit;

namespace ApplicationCore.Features.Emails.Services;

internal class BasicEmailService : IEmailService {

    private readonly ISmtpClientFactory _clientFactory;

    public BasicEmailService(ISmtpClientFactory clientFactory) {
        _clientFactory = clientFactory;
    }

    public async Task<string> SendEmailAsync(Email email) {

        var message = new MimeMessage {
            Sender = MailboxAddress.Parse(email.Sender.Email),
            Subject = email.Subject,
            Body = new BodyBuilder {
                TextBody = email.Body
            }.ToMessageBody()
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
