using ApplicationCore.Features.Labels.Contracts;
using ApplicationCore.Features.Labels.Services;
using ApplicationCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Labels.Handlers;

public class PrintLabelsHandler : CommandHandler<PrintLabelsRequest> {

    private readonly ILogger<PrintLabelsHandler> _logger;
    private readonly ILabelPrinterService _labelPrinterService;

    public PrintLabelsHandler(ILogger<PrintLabelsHandler> logger, ILabelPrinterService labelPrinterService) {
        _logger = logger;
        _labelPrinterService = labelPrinterService;
    }

    public override async Task<Response> Handle(PrintLabelsRequest request) {

        _logger.LogInformation("Printing {Count} labels", request.Labels.Count);
        var printTasks = new List<Task>();
        foreach (var label in request.Labels) {
            printTasks.Add(_labelPrinterService.PrintLabelAsync(label, request.Configuration));
        }

        await Task.WhenAll(printTasks);

        return new Response();

    }

}
