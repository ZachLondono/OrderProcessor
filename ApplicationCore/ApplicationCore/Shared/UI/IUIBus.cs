namespace ApplicationCore.Infrastructure.UI;

public interface IUIBus {

    void Register(IUIListener listener);

    void UnRegister(IUIListener listener);

    void Publish<TNotification>(TNotification notification) where TNotification : IUINotification;

}
