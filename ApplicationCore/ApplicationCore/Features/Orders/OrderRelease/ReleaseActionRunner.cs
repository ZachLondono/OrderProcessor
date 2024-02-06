using Domain.Orders.Entities;
using Domain.Components.ProgressModal;

namespace ApplicationCore.Features.Orders.OrderRelease;

public class ReleaseActionRunner : IActionRunner {

    public Action? ShowProgressBar { get; set; }
    public Action? HideProgressBar { get; set; }
    public Action<int>? SetProgressBarValue { get; set; }
    public Action<ProgressLogMessage>? PublishProgressMessage { get; set; }

    private readonly ReleaseService _service;
    private readonly List<Order> _orders;
    private readonly ReleaseConfiguration _configuration;

    public ReleaseActionRunner(ReleaseService service, List<Order> orders, ReleaseConfiguration configuration) {
        _service = service;
        _orders = orders;
        _configuration = configuration;

        _service.ShowProgressBar += () => ShowProgressBar?.Invoke();
        _service.HideProgressBar += () => HideProgressBar?.Invoke();
        _service.SetProgressBarValue += (val) => SetProgressBarValue?.Invoke(val);
        _service.OnProgressReport += (msg) => PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, msg));
        _service.OnError += (msg) => PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, msg));
        _service.OnActionComplete += (msg) => PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Success, msg));
        _service.OnFileGenerated += (msg) => PublishProgressMessage?.Invoke(new(ProgressLogMessageType.FileCreated, msg));
    }

    public async Task Run() {
        await _service.Release(_orders, _configuration);
    }
}
