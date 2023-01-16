using ApplicationCore.Features.Emails.Domain;
using MimeKit;
using NSubstitute;
using FluentAssertions;
using ApplicationCore.Features.Shared;
using ApplicationCore.Features.Emails.Services;

namespace ApplicationCore.Tests.Unit.Emails;

public class TemplatedEmailServiceTests {

    private readonly TemplatedEmailService _sut;
    private readonly IEmailService _emailService = Substitute.For<IEmailService>();
    private readonly ITemplateService _templateService = Substitute.For<ITemplateService>();

    public TemplatedEmailServiceTests() {
        _sut = new(_emailService, _templateService);
    }

    [Fact]
    public async Task SendEmail_ShouldSendCorrectEmail_WithValidEmail() {

        // Arrange
        string name = "Name";
        string senderEmail = "Email";
        string password = "Password";
        string host = "Host";
        int port = 123;
        var sender = new EmailSender(name, senderEmail, password, host, port);

        var recipients = new List<string>() {
            "Recipient"
        };

        string subject = "Subject @Model.A";
        string finalSubject = "Subject A";
        string body = "Body @Model.B";
        string finalBody = "Body B";
        var email = new Email(sender, recipients, subject, body);

        string serverResponse = "Server Response";
        _emailService.SendEmailAsync(email).ReturnsForAnyArgs(serverResponse);

        var model = new {
            A = "A",
            B = "B"
        };

        _templateService.FillTemplate(subject, model).Returns(finalSubject);
        _templateService.FillTemplate(body, model).Returns(finalBody);

        // Act
        var result = await _sut.SendEmailAsync(email, model);

        // Assert
        result.Should().Be(serverResponse);
        await _emailService.Received()
                            .SendEmailAsync(
                                Arg.Is<Email>(m =>
                                    m.Body.Equals(finalBody) &&
                                    m.Subject.Equals(finalSubject) &&
                                    m.Sender.Equals(sender) &&
                                    m.Recipients.Contains(recipients.First())
                                )
                            );

    }

}
