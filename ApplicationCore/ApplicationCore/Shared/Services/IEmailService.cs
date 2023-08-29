using MimeKit;

namespace ApplicationCore.Shared.Services;

public interface IEmailService {
	public Task<MailboxAddress> GetSenderAsync();
	public Task<string> SendMessageAsync(MimeMessage message);
}
