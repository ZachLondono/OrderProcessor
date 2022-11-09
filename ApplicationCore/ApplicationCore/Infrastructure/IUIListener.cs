namespace ApplicationCore.Infrastructure;

public interface IUIListener<TNotification> where TNotification : IUINotification {

    public void Handle(TNotification notification);

}
