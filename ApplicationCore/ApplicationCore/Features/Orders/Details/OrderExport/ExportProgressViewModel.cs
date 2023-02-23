
namespace ApplicationCore.Features.Orders.Details.OrderExport;

internal class ExportProgressViewModel {

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

    private readonly ExportService _service;

    public ExportProgressViewModel(ExportService service) {
        _service = service;
    }

    public async Task ExportOrder(ExportConfiguration configuration) {

        InProgress = true;

        await _service.Export(configuration);
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