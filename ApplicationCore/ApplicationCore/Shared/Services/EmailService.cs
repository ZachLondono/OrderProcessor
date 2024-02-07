using ApplicationCore.Shared.Settings;
using Domain.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ApplicationCore.Shared.Services;

public class EmailService : IEmailService {

    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> option) {
        _settings = option.Value;
    }

    public MailboxAddress GetSender() {
        return new(_settings.SenderName, _settings.SenderEmail);
    }

    public async Task<string> SendMessageAsync(MimeMessage message) {

        string password;
        try {
            password = UserDataProtection.Unprotect(_settings.ProtectedPassword);
        } catch (Exception ex) {
            throw new InvalidOperationException("Failed to get email sender credentials. Check that email sender is properly configured.", ex);
        }

        using var client = new SmtpClient();
        client.Connect(_settings.Host, _settings.Port, SecureSocketOptions.Auto);
        client.Authenticate(_settings.SenderEmail, password);

        var response = await client.SendAsync(message);

        await client.DisconnectAsync(true);

        return response;

    }

}
