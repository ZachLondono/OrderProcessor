using ApplicationCore.Features.Shared.Components.ProgressModal;

namespace ApplicationCore.Features.Orders.Details.OrderExport;

internal class ExportActionRunner : IActionRunner {

    private readonly ExportService _exportService;
    private readonly ExportConfiguration _configuration;

    public ExportActionRunner(ExportService exportService, ExportConfiguration configuration) {
        _exportService = exportService;
        _configuration = configuration;
        _exportService.OnProgressReport += (msg) => PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, msg));
        _exportService.OnError += (msg) => PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, msg));
        _exportService.OnFileGenerated += (msg) => PublishProgressMessage?.Invoke(new(ProgressLogMessageType.FileCreated, msg));
        _exportService.OnActionComplete += (msg) => PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Success, msg));
    }

    public Action<ProgressLogMessage>? PublishProgressMessage { get; set; }

    public async Task Run() => await _exportService.Export(_configuration);

}
