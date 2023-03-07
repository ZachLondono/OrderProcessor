using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.Invoice;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.JobSummary;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.PackingList;
using ApplicationCore.Features.Orders.Details.Shared;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.State;
using ApplicationCore.Features.Shared.Services;
using QuestPDF.Fluent;
using static ApplicationCore.Features.Companies.Contracts.CompanyDirectory;

namespace ApplicationCore.Features.Orders.Details.OrderRelease;

internal class ReleaseService {

    public Action<string>? OnProgressReport;
    public Action<string>? OnError;
    public Action<string>? OnActionComplete;

    private readonly InvoiceHandler _invoiceHandler;
    private readonly PackingListHandler _packingListHandler;
    private readonly OrderState _orderState;
    private readonly IFileReader _fileReader;
    private readonly GenerateReleaseForSelectedJobs.Handler _cncReleaseHandler;
    private readonly GetCustomerByIdAsync _getCustomerByIdAsync;
    private readonly GetVendorByIdAsync _getVendorByIdAsync;

    public ReleaseService(InvoiceHandler invoiceHandler, PackingListHandler packingListHandler, OrderState orderState, IFileReader fileReader,
                        GenerateReleaseForSelectedJobs.Handler cncReleaseHandler, GetCustomerByIdAsync getCustomerByIdAsync, GetVendorByIdAsync getVendorByIdAsync) {
        _invoiceHandler = invoiceHandler;
        _packingListHandler = packingListHandler;
        _cncReleaseHandler = cncReleaseHandler;
        _orderState = orderState;
        _fileReader = fileReader;
        _getCustomerByIdAsync = getCustomerByIdAsync;
        _getVendorByIdAsync = getVendorByIdAsync;
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

        await GenerateInvoice(configuration, order);
        await GeneratePackingList(configuration, order);

        List<IDocumentDecorator> cncDecorators = new();
        if (configuration.GenerateCNCRelease && configuration.CNCDataFilePath is not null) {

            var vendor = await _getVendorByIdAsync(order.VendorId);
            var customer = await _getCustomerByIdAsync(order.CustomerId);

            var response = await _cncReleaseHandler.Handle(new GenerateReleaseForSelectedJobs.Command(order.Id, $"{order.Number} {order.Name}", customer?.Name ?? "", vendor?.Name ?? "", DateTime.Now, configuration.CNCDataFilePath));

            response.OnSuccess(result => cncDecorators.AddRange(result.Decorators));

            OnActionComplete?.Invoke("CNC release complete");

        } else {
            OnProgressReport?.Invoke("Skipping CNC release, because it was unchecked");
        }


        bool generatePDF = false;

        IDocumentDecorator? summaryDecorator = null;
        if (configuration.GenerateJobSummary) {
            summaryDecorator = new JobSummaryDocumentDecorator(order);
            generatePDF = true;
        }

        List<Document> documents = new();
        if (cncDecorators.Any()) {

            foreach (var decorator in cncDecorators) {

                documents.Add(Document.Create(doc => {

                    summaryDecorator?.Decorate(doc);

                    decorator.Decorate(doc);

                }));

            }

        } else if (summaryDecorator is not null) {

            documents.Add(Document.Create(summaryDecorator.Decorate));

        }

        List<string> generatedFiles = new();
        if (generatePDF) {

            foreach (var document in documents) {

                try {
                    var filePath = _fileReader.GetAvailableFileName(configuration.OutputDirectory, $"{order.Number} {order.Name}", ".pdf");
                    document.GeneratePdf(filePath);
                    generatedFiles.Add(filePath);
                } catch (Exception ex) {
                    generatePDF = false;
                    OnError?.Invoke($"Error generating pdf {ex.Message}");
                }

            }

        }

        if (configuration.SendEmail) {

            // TODO: attach pdf to email

        }

        OnActionComplete?.Invoke("Release Complete");

    }

    private async Task GeneratePackingList(ReleaseConfiguration configuration, Order order) {

        if (configuration.OutputDirectory is null) {
            OnError?.Invoke("No output directory set");
            return;
        }

        if (configuration.GeneratePackingList) {
            await _packingListHandler.Handle(order, configuration.OutputDirectory);
            OnActionComplete?.Invoke("Packing list generated");
        } else {
            OnProgressReport?.Invoke("Skipping packing list, because it was unchecked");
        }
    }

    private async Task GenerateInvoice(ReleaseConfiguration configuration, Order order) {

        if (configuration.OutputDirectory is null) {
            OnError?.Invoke("No output directory set");
            return;
        }

        if (configuration.GenerateInvoice) {
            await _invoiceHandler.Handle(order, configuration.OutputDirectory);
            OnActionComplete?.Invoke("Invoice generated");
        } else {
            OnProgressReport?.Invoke("Skipping invoice, because it was unchecked");
        }
    }
}
