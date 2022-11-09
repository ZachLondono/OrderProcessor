using MediatR;

namespace ApplicationCore.Infrastructure;

public abstract class DomainListener<TNotification> : INotificationHandler<TNotification> where TNotification : IDomainNotification {

    public abstract Task Handle(TNotification notification);

    public Task Handle(TNotification notification, CancellationToken cancellationToken) => Handle(notification);

}