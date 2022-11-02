using MimeKit;

namespace ApplicationCore.Features.Emails;

internal class MailKitSmtpClient : ISmtpClient {

    private readonly MailKit.Net.Smtp.SmtpClient _client;

    public MailKitSmtpClient(MailKit.Net.Smtp.SmtpClient client) => _client = client;

    public Task AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
        => _client.AuthenticateAsync(username, password, cancellationToken);

    public Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default)
        => _client.ConnectAsync(host: host, port: port, cancellationToken: cancellationToken);

    public Task DisconnectAsync(bool quit, CancellationToken cancellationToken = default)
        => _client.DisconnectAsync(quit, cancellationToken);

    public Task<string> SendAsync(MimeMessage message, CancellationToken cancellationToken = default)
        => _client.SendAsync(message, cancellationToken);

    public void Dispose() => _client.Dispose();

}
