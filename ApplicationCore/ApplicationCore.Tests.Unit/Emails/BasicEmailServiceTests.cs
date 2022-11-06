using ApplicationCore.Features.Emails;
using ApplicationCore.Features.Emails.Domain;
using ApplicationCore.Features.Emails.Services;
using FluentAssertions;
using MimeKit;
using NSubstitute;
using NSubstitute.ReceivedExtensions;

namespace ApplicationCore.Tests.Unit.Emails;

public class BasicEmailServiceTests {

    private readonly BasicEmailService _sut;
    private readonly ISmtpClientFactory _factory = Substitute.For<ISmtpClientFactory>();
    private readonly ISmtpClient _client = Substitute.For<ISmtpClient>();

    public BasicEmailServiceTests() {
        _sut = new(_factory);

        _factory.CreateClient().Returns(_client);
    }

    [Fact]
    public async Task SendEmail_ShouldSendCorrectEmail_WithValidEmail() {

        // Arrange
        string name = "Name";
        string senderEmail = "email@email.com";
        string password = "Password";
        string host = "Host";
        int port = 123;
        var sender = new EmailSender(name, senderEmail, password, host, port);

        var recipients = new List<string>() {
            "Recipient"
        };

        string subject = "Subject";
        string body = "Body";
        var email = new Email(sender, recipients, subject, body);

        var message = new MimeMessage {
            Sender = MailboxAddress.Parse(email.Sender.Email),
            Subject = email.Subject,
            Body = new BodyBuilder {
                TextBody = email.Body
            }.ToMessageBody()
        };
        var recipientMBA = MailboxAddress.Parse(email.Recipients[0]);
        message.To.Add(recipientMBA);

        string serverResponse = "Server Response";
        _client.SendAsync(message).ReturnsForAnyArgs(serverResponse);

        // Act
        var result = await _sut.SendEmailAsync(email);

        // Assert
        result.Should().Be(serverResponse);
        await _client.Received(1).ConnectAsync(host, port);
        await _client.Received(1).AuthenticateAsync(senderEmail, password);
        await _client.Received(1).DisconnectAsync(true);
        await _client.ReceivedWithAnyArgs(1).SendAsync(new MimeMessage());

    }

}
