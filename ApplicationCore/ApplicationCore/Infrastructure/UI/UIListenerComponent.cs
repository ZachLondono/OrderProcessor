namespace ApplicationCore.Infrastructure.UI;

public abstract class UIListenerComponent<TNotification> : BaseListenerComponent, IUIListener<TNotification> where TNotification : IUINotification {

    public abstract void Handle(TNotification notification);

}