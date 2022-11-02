using MimeKit;

namespace ApplicationCore.Features.Emails;

public interface ISmtpClient : IDisposable {

    Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default);
    Task AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<string> SendAsync(MimeMessage message, CancellationToken cancellationToken = default);
    Task DisconnectAsync(bool quit, CancellationToken cancellationToken = default);

}
