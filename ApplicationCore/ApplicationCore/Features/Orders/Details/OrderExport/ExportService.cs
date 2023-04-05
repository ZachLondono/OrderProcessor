using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Orders.Details.OrderExport.Handlers;
using ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport;
using ApplicationCore.Features.Orders.Shared.State;
using ApplicationCore.Features.Shared.Services;
using Microsoft.Extensions.Options;

namespace ApplicationCore.Features.Orders.Details.OrderExport;

internal class ExportService {

    private const string EXT_OUTPUT_DIRECTORY = @"C:\CP3\CPDATA";

    public Action<string>? OnProgressReport;
    public Action<string>? OnFileGenerated;
    public Action<string>? OnError;
    public Action<string>? OnActionComplete;

    private readonly OrderState _orderState;
    private readonly DoorOrderHandler _doorOrderHandler;
    private readonly DovetailOrderHandler _dovetailOrderHandler;
    private readonly ExtOrderHandler _extOrderHandler;
    private readonly ExportOptions _options;
    private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerByIdAsync;
    private readonly IFileReader _fileReader;

    public ExportService(OrderState orderState, DoorOrderHandler doorOrderHandler, DovetailOrderHandler dovetailOrderHandler, ExtOrderHandler extOrderHandler, IOptions<ExportOptions> options, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync, IFileReader fileReader) {
        _orderState = orderState;
        _doorOrderHandler = doorOrderHandler;
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

        if (configuration.FillMDFDoorOrder) {
            if (File.Exists(_options.MDFDoorTemplateFilePath)) {
                await _doorOrderHandler.Handle(order, _options.MDFDoorTemplateFilePath, outputDir);
            } else {
                OnError?.Invoke($"Could not find MDF order template file '{_options.MDFDoorTemplateFilePath}'");
            }
        } else {
            OnProgressReport?.Invoke("Not generating MDF door order");
        }

        if (configuration.FillDovetailOrder) {
            if (File.Exists(_options.DovetailTemplateFilePath)) {
                await _dovetailOrderHandler.Handle(order, _options.DovetailTemplateFilePath, outputDir);
            } else {
                OnError?.Invoke($"Could not find dovetail order template file '{_options.DovetailTemplateFilePath}'");
            }
        } else {
            OnProgressReport?.Invoke("Not filling dovetail drawer box order");
        }

        if (configuration.GenerateEXT) {
            await _extOrderHandler.Handle(order, EXT_OUTPUT_DIRECTORY);
        } else {
            OnProgressReport?.Invoke("Not generating EXT file");
        }

        OnActionComplete?.Invoke("Export Complete");

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
