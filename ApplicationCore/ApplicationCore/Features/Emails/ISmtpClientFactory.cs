namespace ApplicationCore.Features.Emails;

public interface ISmtpClientFactory {

    public ISmtpClient CreateClient();

}
