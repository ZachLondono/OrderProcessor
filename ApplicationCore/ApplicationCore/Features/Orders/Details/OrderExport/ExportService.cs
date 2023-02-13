using ApplicationCore.Features.Orders.Details.OrderExport.Handlers;
using ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport;
using ApplicationCore.Features.Orders.Shared.State;

namespace ApplicationCore.Features.Orders.Details.OrderExport;

internal class ExportService {

    private readonly OrderState _orderState;
    private readonly DoorOrderHandler _doorOrderHandler;
    private readonly DovetailOrderHandler _dovetailOrderHandler;
    private readonly ExtOrderHandler _extOrderHandler;

    public ExportService(OrderState orderState, DoorOrderHandler doorOrderHandler, DovetailOrderHandler dovetailOrderHandler, ExtOrderHandler extOrderHandler) {
        _orderState = orderState;
        _doorOrderHandler = doorOrderHandler;
        _dovetailOrderHandler = dovetailOrderHandler;
        _extOrderHandler = extOrderHandler;
    }

    public async Task Export(ExportConfiguration configuration) {

        if (_orderState.Order is null || configuration.OutputDirectory is null) {
            return;
        }

        var order = _orderState.Order;
        var outputDir = configuration.OutputDirectory;

        if (configuration.FillMDFDoorOrder && configuration.MDFDoorTemplateFilePath is string mdfTemplate) {

            await _doorOrderHandler.Handle(order, mdfTemplate, outputDir);

        }

        if (configuration.FillDovetailOrder && configuration.DovetailTemplateFilePath is string dovetailTempalte) {

            await _dovetailOrderHandler.Handle(order, dovetailTempalte, outputDir);

        }

        if (configuration.GenerateEXT) {

            await _extOrderHandler.Handle(order, outputDir);

        }

    }

}
