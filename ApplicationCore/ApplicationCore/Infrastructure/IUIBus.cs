namespace ApplicationCore.Infrastructure;

public interface IUIBus {

    void Register(IListener listener);

    void UnRegister(IListener listener);

    void Publish<TNotification>(TNotification notification) where TNotification : IUINotification;

}
