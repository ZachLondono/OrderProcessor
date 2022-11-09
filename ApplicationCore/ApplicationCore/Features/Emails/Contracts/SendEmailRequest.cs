using ApplicationCore.Features.Emails.Domain;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.Emails.Contracts;

public class SendEmailRequest : ICommand<SendEmailResponse> {
    public Email Email { get; init; }
    public SendEmailRequest(Email email) {
        Email = email;
    }
}
