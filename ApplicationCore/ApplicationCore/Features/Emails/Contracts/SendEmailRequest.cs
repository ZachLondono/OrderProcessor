using ApplicationCore.Features.Emails.Domain;
using ApplicationCore.Infrastructure;
using MediatR;

namespace ApplicationCore.Features.Emails.Contracts;

public class SendEmailRequest : IQuery<SendEmailResponse> {
    public Email Email { get; init; }
    public SendEmailRequest(Email email) {
        Email = email;
    }
}
