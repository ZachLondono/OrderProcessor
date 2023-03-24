using ApplicationCore.Features.Orders.Details.OrderExport.Handlers;
using ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport;
using ApplicationCore.Features.Orders.Shared.State;
using Microsoft.Extensions.Options;

namespace ApplicationCore.Features.Orders.Details.OrderExport;

internal class ExportService {

    private const string EXT_OUTPUT_DIRECTORY = @"C:\CP3\CPDATA";

    private readonly OrderState _orderState;
    private readonly DoorOrderHandler _doorOrderHandler;
    private readonly DovetailOrderHandler _dovetailOrderHandler;
    private readonly ExtOrderHandler _extOrderHandler;
    private readonly ExportOptions _options;

    public ExportService(OrderState orderState, DoorOrderHandler doorOrderHandler, DovetailOrderHandler dovetailOrderHandler, ExtOrderHandler extOrderHandler, IOptions<ExportOptions> options) {
        _orderState = orderState;
        _doorOrderHandler = doorOrderHandler;
        _dovetailOrderHandler = dovetailOrderHandler;
        _extOrderHandler = extOrderHandler;
        _options = options.Value;
    }

    public async Task Export(ExportConfiguration configuration) {

        if (_orderState.Order is null || configuration.OutputDirectory is null) {
            return;
        }

        var order = _orderState.Order;
        var outputDir = configuration.OutputDirectory;

        if (configuration.FillMDFDoorOrder) {

            if (File.Exists(_options.MDFDoorTemplateFilePath)) {
                await _doorOrderHandler.Handle(order, _options.MDFDoorTemplateFilePath, outputDir);
            }

        }

        if (configuration.FillDovetailOrder) {

            if (File.Exists(_options.DovetailTemplateFilePath)) {
                await _dovetailOrderHandler.Handle(order, _options.DovetailTemplateFilePath, outputDir);
            }

        }

        if (configuration.GenerateEXT) {

            await _extOrderHandler.Handle(order, EXT_OUTPUT_DIRECTORY);

        }

    }

}
