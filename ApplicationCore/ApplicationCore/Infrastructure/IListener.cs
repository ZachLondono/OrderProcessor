namespace ApplicationCore.Infrastructure;

public interface IListener {

}

public interface IListener<TNotification> where TNotification : IUINotification {

    public void Handle(TNotification notification);

}