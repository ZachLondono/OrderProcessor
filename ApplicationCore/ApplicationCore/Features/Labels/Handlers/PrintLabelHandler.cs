using ApplicationCore.Features.Labels.Contracts;
using ApplicationCore.Features.Labels.Services;
using ApplicationCore.Infrastructure.Bus;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Labels.Handlers;

public class PrintLabelHandler : CommandHandler<PrintLabelRequest> {

    private readonly ILogger<PrintLabelHandler> _logger;
    private readonly ILabelPrinterService _labelPrinterService;

    public PrintLabelHandler(ILogger<PrintLabelHandler> logger, ILabelPrinterService labelPrinterService) {
        _logger = logger;
        _labelPrinterService = labelPrinterService;
    }

    public override async Task<Response> Handle(PrintLabelRequest request) {

        _logger.LogInformation("Printing a templated label");
        await _labelPrinterService.PrintLabelAsync(request.Label, request.Configuration);

        return new Response();

    }

}
