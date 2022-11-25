using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.Orders.Loader;

internal class LoadingMessagePublisher {

    private readonly IUIBus _uiBus;

    public LoadingMessagePublisher(IUIBus uiBus) {
        _uiBus = uiBus;
    }

    public void PublishWarning(string message) {
        _uiBus.Publish(new OrderLoadMessage() {
            Severity = MessageSeverity.Warning,
            Message = message
        });
    }

    public void PublishError(string message) {
        _uiBus.Publish(new OrderLoadMessage() {
            Severity = MessageSeverity.Error,
            Message = message
        });
    }

}
