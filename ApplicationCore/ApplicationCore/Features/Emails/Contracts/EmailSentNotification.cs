using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.Emails.Contracts;

public record EmailSentNotification : IDomainNotification {

    public string Recipient { get; init; }

    public EmailSentNotification(string recipient) {
        Recipient = recipient;
    }

}
