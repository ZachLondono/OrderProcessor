namespace ApplicationCore.Features.Emails;

internal class MailKitSmtpClientFactory : ISmtpClientFactory {

    public ISmtpClient CreateClient() => new MailKitSmtpClient(new());

}
