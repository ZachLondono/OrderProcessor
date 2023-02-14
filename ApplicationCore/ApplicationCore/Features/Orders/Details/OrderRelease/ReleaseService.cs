using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.Invoice;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.JobSummary;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.PackingList;
using ApplicationCore.Features.Orders.Shared.State;
using MoreLinq;

namespace ApplicationCore.Features.Orders.Details.OrderRelease;

internal class ReleaseService {

    public Action<string>? OnProgressReport;
    public Action<string>? OnError;
    public Action<string>? OnActionComplete;

    private readonly InvoiceHandler _invoiceHandler;
    private readonly PackingListHandler _packingListHandler;
    private readonly JobSummaryHandler _jobSummaryHandler;
    private readonly GenerateReleaseForSelectedJobs.Handler _cncReleaseHandler;
    private readonly OrderState _orderState;

    public ReleaseService(InvoiceHandler invoiceHandler, PackingListHandler packingListHandler, JobSummaryHandler jobSummaryHandler, GenerateReleaseForSelectedJobs.Handler cncReleaseHandler, OrderState orderState) {
        _invoiceHandler = invoiceHandler;
        _packingListHandler = packingListHandler;
        _jobSummaryHandler = jobSummaryHandler;
        _cncReleaseHandler = cncReleaseHandler;
        _orderState = orderState;
    }

    public async Task Release(ReleaseConfiguration configuration) {

        if (_orderState.Order is null) {
            OnError?.Invoke("No order selected");
            return;
        }

        if (configuration.OutputDirectory is null) {
            OnError?.Invoke("No output directory set");
            return;
        }

        var order = _orderState.Order;

        if (configuration.GenerateInvoice) {
            await _invoiceHandler.Handle(order, configuration.OutputDirectory);
            OnActionComplete?.Invoke("Invoice generated");
        } else {
            OnProgressReport?.Invoke("Skipping invoice, because it was unchecked");
        }

        if (configuration.GeneratePackingList) {
            await _packingListHandler.Handle(order, configuration.OutputDirectory);
            OnActionComplete?.Invoke("Packing list generated");
        } else {
            OnProgressReport?.Invoke("Skipping packing list, because it was unchecked");
        }

        if (configuration.GenerateCNCRelease && configuration.CNCDataFilePath is not null && configuration.CNCJobs is not null && configuration.CNCJobs.Any()) {

            var response = await _cncReleaseHandler.Handle(new GenerateReleaseForSelectedJobs.Command(order.Id, $"{order.Number} {order.Name}", order.Customer.Name, "Vendor Name", DateTime.Now, configuration.CNCDataFilePath, configuration.CNCJobs));

            response.OnSuccess(result => {
                result.FilesWritten.ForEach(file => OnProgressReport?.Invoke($"File written '{file}'"));
            });

            OnActionComplete?.Invoke("CNC release complete");

        } else {
            OnProgressReport?.Invoke("Skipping CNC release, because it was unchecked");
        }

        if (configuration.GenerateJobSummary && configuration.JobSummaryTemplate is not null) {

            _jobSummaryHandler.Handle(order, configuration.JobSummaryTemplate, configuration.OutputDirectory);

        }

        if (configuration.SendEmail) {



        }

        OnActionComplete?.Invoke("Release Complete");

    }

}
