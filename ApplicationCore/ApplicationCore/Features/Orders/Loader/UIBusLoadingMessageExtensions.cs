using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.Orders.Loader;

public static class UIBusLoadingMessageExtensions {

    public static void PublishWarning(this IUIBus uiBus, string message) {
        uiBus.Publish(new OrderLoadMessage() {
            Severity = MessageSeverity.Warning,
            Message = message
        });
    }

    public static void PublishError(this IUIBus uiBus, string message) {
        uiBus.Publish(new OrderLoadMessage() {
            Severity = MessageSeverity.Error,
            Message = message
        });
    }

}
