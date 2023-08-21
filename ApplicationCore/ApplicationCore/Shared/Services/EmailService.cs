using ApplicationCore.Shared.Data;
using ApplicationCore.Shared.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ApplicationCore.Shared.Services;

public class EmailService : IEmailService {

	private readonly Email _emailSettings;

	public EmailService(IOptions<Email> options) {
		_emailSettings = options.Value;
	}

	public MailboxAddress GetSender() => new(_emailSettings.SenderName, _emailSettings.SenderEmail);

	public async Task<string> SendMessageAsync(MimeMessage message) {

		using var client = new SmtpClient();
		client.Connect(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.Auto);
		client.Authenticate(_emailSettings.SenderEmail, UserDataProtection.Unprotect(_emailSettings.ProtectedPassword));

		var response = await client.SendAsync(message);

		await client.DisconnectAsync(true);

		return response;

	}

}
