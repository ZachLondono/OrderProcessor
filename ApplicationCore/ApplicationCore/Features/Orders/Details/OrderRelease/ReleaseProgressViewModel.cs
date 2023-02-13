using System.Collections.ObjectModel;

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

    public ObservableCollection<ProgressLogMessage> Messages { get; } = new();

    private readonly ReleaseService _service;

    public ReleaseProgressViewModel(ReleaseService service) {
        _service = service;

        _service.OnProgressReport += (message) => Messages.Add(new(LogMessageType.Info, message));
        _service.OnError += (message) => Messages.Add(new(LogMessageType.Error, message));
        _service.OnActionComplete += (message) => Messages.Add(new(LogMessageType.Success, message));
    }

    public async Task ReleaseOrder(ReleaseConfiguration configuration) {

        InProgress = true;

        await _service.Release(configuration);
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
