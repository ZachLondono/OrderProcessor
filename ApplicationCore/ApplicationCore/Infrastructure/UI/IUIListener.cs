namespace ApplicationCore.Infrastructure.UI;

public interface IUIListener {

}

public interface IUIListener<TNotification> where TNotification : IUINotification {

    public void Handle(TNotification notification);

}
