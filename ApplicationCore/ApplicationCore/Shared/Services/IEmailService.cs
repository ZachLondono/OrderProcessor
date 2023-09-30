using MimeKit;

namespace ApplicationCore.Shared.Services;

public interface IEmailService {
    public MailboxAddress GetSender();
    public Task<string> SendMessageAsync(MimeMessage message);
}
