using System.Diagnostics;

namespace ApplicationCore.Infrastructure.UI;

public class UIBus : IUIBus {

    // Maybe use hashset
    private readonly List<IUIListener> _listeners = new();

    public void Publish<TNotification>(TNotification notification) where TNotification : IUINotification {

        var listeners = _listeners.Where(l => l is IUIListener<TNotification>).Cast<IUIListener<TNotification>>();

        Debug.WriteLine($"Publishing event {notification.GetType().Name} to {listeners.Count()} listeners");

        foreach (IUIListener<TNotification> listener in listeners) {
            listener.Handle(notification);
        }

    }

    public void Register(IUIListener listener) {
        Debug.WriteLine($"Registering listener {listener.GetType().Name}");
        _listeners.Add(listener);
    }

    public void UnRegister(IUIListener listener) {
        Debug.WriteLine($"UnRegistering listener {listener.GetType().Name}");
        _listeners.Remove(listener);
    }
}