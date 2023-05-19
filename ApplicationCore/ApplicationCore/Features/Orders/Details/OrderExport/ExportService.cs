using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Orders.Details.OrderExport.Handlers.DoorOrderExport;
using ApplicationCore.Features.Orders.Details.OrderExport.Handlers.DovetailOrderExport;
using ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.State;
using ApplicationCore.Features.Shared;
using ApplicationCore.Features.Shared.Services;
using ApplicationCore.Infrastructure.Bus;
using Microsoft.Extensions.Options;

namespace ApplicationCore.Features.Orders.Details.OrderExport;

// TODO: Merge ExportService and ExportWidgetViewModel
internal class ExportService {

    private const string EXT_OUTPUT_DIRECTORY = @"C:\CP3\CPDATA";

    public Action<string>? OnProgressReport;
    public Action<string>? OnFileGenerated;
    public Action<string>? OnError;
    public Action<string>? OnActionComplete;

    private readonly IBus _bus;
    private readonly OrderState _orderState;
    private readonly DovetailOrderHandler _dovetailOrderHandler;
    private readonly ExtOrderHandler _extOrderHandler;
    private readonly ExportOptions _options;
    private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerByIdAsync;
    private readonly IFileReader _fileReader;

    public ExportService(IBus bus, OrderState orderState, DovetailOrderHandler dovetailOrderHandler, ExtOrderHandler extOrderHandler, IOptions<ExportOptions> options, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync, IFileReader fileReader) {
        _bus = bus;
        _orderState = orderState;
        _dovetailOrderHandler = dovetailOrderHandler;
        _extOrderHandler = extOrderHandler;
        _options = options.Value;
        _getCustomerByIdAsync = getCustomerByIdAsync;
        _fileReader = fileReader;
    }

    public async Task Export(ExportConfiguration configuration) {

        if (_orderState.Order is null || configuration.OutputDirectory is null) {
            return;
        }

        var order = _orderState.Order;

        var outputDir = configuration.OutputDirectory;
        var customerName = await GetCustomerName(order.CustomerId);
        outputDir = ReplaceTokensInDirectory(customerName, outputDir);

        await GenerateMDFOrders(configuration, order, outputDir);

        await GenerateDovetailOrders(configuration, order, outputDir);

        await GenerateEXT(configuration, order);

        OnActionComplete?.Invoke("Export Complete");

    }

    private async Task GenerateMDFOrders(ExportConfiguration configuration, Order order, string outputDir) {

        if (!configuration.FillMDFDoorOrder) {
            OnProgressReport?.Invoke("Not generating MDF door order");
            return;
        }

        if (!File.Exists(_options.MDFDoorTemplateFilePath)) {
            OnError?.Invoke($"Could not find MDF order template file '{_options.MDFDoorTemplateFilePath}'");
            return;
        }

        OnProgressReport?.Invoke("Generating MDF Door Orders");

        var response = await _bus.Send(new ExportDoorOrder.Command(order, _options.MDFDoorTemplateFilePath, outputDir));

        response.Match(
            result => {
                result.GeneratedFiles.ForEach(f => OnFileGenerated?.Invoke(f));
                if (result.Error is string error) {
                    OnError?.Invoke(error);
                }
            },
            error => OnError?.Invoke($"{error.Title} - {error.Details}")
        );

    }

    private async Task GenerateDovetailOrders(ExportConfiguration configuration, Order order, string outputDir) {
        if (configuration.FillDovetailOrder) {
            if (File.Exists(_options.DovetailTemplateFilePath)) {

                OnProgressReport?.Invoke("Generating Dovetail Drawer Box Orders");
                var result = await _dovetailOrderHandler.Handle(order, _options.DovetailTemplateFilePath, outputDir);
                result.GeneratedFiles.ForEach(f => OnFileGenerated?.Invoke(f));
                if (result.Error is string error) {
                    OnError?.Invoke(error);
                }

            } else {
                OnError?.Invoke($"Could not find dovetail order template file '{_options.DovetailTemplateFilePath}'");
            }
        } else {
            OnProgressReport?.Invoke("Not filling dovetail drawer box order");
        }
    }

    private async Task GenerateEXT(ExportConfiguration configuration, Order order) {
        if (configuration.GenerateEXT) {

            OnProgressReport?.Invoke("Generating EXT File");
            string jobName = string.IsNullOrWhiteSpace(configuration.ExtJobName) ? $"{order.Number} - {order.Name}" : configuration.ExtJobName;
            var file = await _extOrderHandler.Handle(order, jobName, EXT_OUTPUT_DIRECTORY);
            if (file is not null) {
                OnFileGenerated?.Invoke(file);
            } else {
                OnError?.Invoke($"Ext file was not generated");
            }

        } else {
            OnProgressReport?.Invoke("Not generating EXT file");
        }
    }

    public string ReplaceTokensInDirectory(string customerName, string outputDir) {
        var sanitizedName = _fileReader.RemoveInvalidPathCharacters(customerName);
        var result = outputDir.Replace("{customer}", sanitizedName);
        return result;
    }

    private async Task<string> GetCustomerName(Guid customerId) {

        try {

            var customer = await _getCustomerByIdAsync(customerId);

            if (customer is null) {
                return string.Empty;
            }

            return customer.Name;

        } catch {

            return string.Empty;

        }

    }


}
