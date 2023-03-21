using ApplicationCore.Features.Orders.Shared.State;

namespace ApplicationCore.Features.Orders.Details.OrderRelease;

internal class ReleaseProgressViewModel {

    public Action? OnPropertyChanged { get; set; }

    private bool _inProgress;
    public bool InProgress {
        get => _inProgress;
        set {
            _inProgress = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private bool _isComplete;
    public bool IsComplete {
        get => _isComplete;
        set {
            _isComplete = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public Action<ProgressLogMessage>? OnMessagePublished { get; set; }

    private readonly ReleaseService _service;
    private readonly OrderState _orderState;

    public ReleaseProgressViewModel(ReleaseService service, OrderState orderState) {
        _service = service;
        _orderState = orderState;

        _service.OnProgressReport += (message) => OnMessagePublished?.Invoke(new(LogMessageType.Info, message));
        _service.OnFileGenerated += (message) => OnMessagePublished?.Invoke(new(LogMessageType.FileCreated, message));
        _service.OnError += (message) => OnMessagePublished?.Invoke(new(LogMessageType.Error, message));
        _service.OnActionComplete += (message) => OnMessagePublished?.Invoke(new(LogMessageType.Success, message));
    }

    public async Task ReleaseOrder(ReleaseConfiguration configuration) {

        if (_orderState.Order is null) {
            OnMessagePublished?.Invoke(new(LogMessageType.Error, "No order selected"));
            return;
        }

        InProgress = true;

        await _service.Release(_orderState.Order, configuration);
        IsComplete = true;

        InProgress = false;

    }

    public record ProgressLogMessage(LogMessageType Type, string Message);
    public enum LogMessageType {
        Info,
        Error,
        Success,
        FileCreated
    }

}
