namespace ApplicationCore.Infrastructure;

public interface IDomainListener<TNotification> where TNotification : IDomainNotification {

    public Task Handle(TNotification notification);

}