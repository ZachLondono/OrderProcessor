using ApplicationCore.Features.Emails.Domain;
using ApplicationCore.Infrastructure;
using MediatR;

namespace ApplicationCore.Features.Emails.Contracts;

public class SendTemplatedEmailRequest : IQuery<SendEmailResponse> {
    public Email Email { get; init; }
    public object Model { get; init; }
    public SendTemplatedEmailRequest(Email email, object model) {
        Email = email;
        Model = model;
    }
}
