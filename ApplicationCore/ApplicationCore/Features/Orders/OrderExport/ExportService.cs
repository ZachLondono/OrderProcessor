using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Orders.OrderExport.Handlers.DoorOrderExport;
using ApplicationCore.Features.Orders.OrderExport.Handlers.DovetailOrderExport;
using ApplicationCore.Features.Orders.OrderExport.Handlers.ExtExport;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.State;
using ApplicationCore.Shared;
using ApplicationCore.Shared.Services;
using ApplicationCore.Infrastructure.Bus;
using Microsoft.Extensions.Options;
using CADCodeProxy.Machining;
using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.CSVTokens;

namespace ApplicationCore.Features.Orders.OrderExport;

// TODO: Merge ExportService and ExportWidgetViewModel
internal class ExportService {

    public Action<string>? OnProgressReport;
    public Action<string>? OnFileGenerated;
    public Action<string>? OnError;
    public Action<string>? OnActionComplete;

    private readonly IBus _bus;
    private readonly OrderState _orderState;
    private readonly ExportOptions _options;
    private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerByIdAsync;
    private readonly IFileReader _fileReader;

    public ExportService(IBus bus, OrderState orderState, IOptions<ExportOptions> options, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync, IFileReader fileReader) {
        _bus = bus;
        _orderState = orderState;
        _options = options.Value;
        _getCustomerByIdAsync = getCustomerByIdAsync;
        _fileReader = fileReader;
    }

    public async Task Export(ExportConfiguration configuration) {

        if (_orderState.Order is null || configuration.OutputDirectory is null) {
            return;
        }

        var order = _orderState.Order;

        var customerName = await GetCustomerName(order.CustomerId);
        var outputDir = ReplaceTokensInDirectory(customerName, configuration.OutputDirectory);

        await GenerateMDFOrders(configuration, order, outputDir);

        await GenerateDovetailOrders(configuration, order, outputDir);

        await GenerateEXT(configuration, order, _options.EXTOutputDirectory);

        await GenerateCSV(configuration, order, customerName, _options.CSVOutputDirectory);

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
                if (result.GeneratedFiles.Any()) {
                    result.GeneratedFiles.ForEach(f => OnFileGenerated?.Invoke(f));
                } else {
                    OnError?.Invoke("No MDF door order files generated");
                }
                if (result.Error is string error) {
                    OnError?.Invoke(error);
                }
            },
            error => OnError?.Invoke($"{error.Title} - {error.Details}")
        );

    }

    private async Task GenerateDovetailOrders(ExportConfiguration configuration, Order order, string outputDir) {

        if (!configuration.FillDovetailOrder) {
            OnProgressReport?.Invoke("Not filling dovetail drawer box order");
            return;
        }

        if (!File.Exists(_options.DovetailTemplateFilePath)) {
            OnError?.Invoke($"Could not find dovetail order template file '{_options.DovetailTemplateFilePath}'");
            return;
        }

        OnProgressReport?.Invoke("Generating Dovetail Drawer Box Orders");

        var response = await _bus.Send(new ExportDovetailOrder.Command(order, _options.DovetailTemplateFilePath, outputDir));

        response.Match(
            result => {
                if (result.GeneratedFiles.Any()) {
                    result.GeneratedFiles.ForEach(f => OnFileGenerated?.Invoke(f));
                } else {
                    OnError?.Invoke("No dovetail order files generated");
                }
                if (result.Error is string error) {
                    OnError?.Invoke(error);
                }
            },
            error => OnError?.Invoke($"{error.Title} - {error.Details}")
        );

    }

    private async Task GenerateEXT(ExportConfiguration configuration, Order order, string outputDir) {

        if (!configuration.GenerateEXT) {
            OnProgressReport?.Invoke("Not generating EXT file");
            return;
        }

        if (!Directory.Exists(outputDir)) {
            OnError?.Invoke("EXT output directory does not exist");
            return;
        }

        OnProgressReport?.Invoke("Generating EXT File");
        string jobName = string.IsNullOrWhiteSpace(configuration.ExtJobName) ? $"{order.Number} - {order.Name}" : configuration.ExtJobName;

        var response = await _bus.Send(new ExportEXT.Command(order, jobName.Trim(), outputDir));

        response.Match(
            file => OnFileGenerated?.Invoke(file),
            error => OnError?.Invoke($"{error.Title} - {error.Details}")
        );

    }

    private async Task GenerateCSV(ExportConfiguration configuration, Order order, string customerName, string outputDir) {

        if (!configuration.GenerateCSV) {
            OnProgressReport?.Invoke("Not generating CSV file");
            return;
        }

        if (!Directory.Exists(outputDir)) {
            OnError?.Invoke("CSV output directory does not exist");
            return;
        }

        var parts = order.Products
            .Where(p => p is ICNCPartContainer)
            .Cast<ICNCPartContainer>()
            .SelectMany(p => p.GetCNCParts())
            .ToArray();

        parts.ForEach(p => p.InfoFields.Add("CustomerInfo1", customerName));

        if (!parts.Any()) {
            OnError?.Invoke("No parts in order to write to CSV");
            return;
        }

        OnProgressReport?.Invoke("Generating CSV file");

        string jobName = string.IsNullOrWhiteSpace(configuration.CsvJobName) ? $"{order.Number} - {order.Name}" : configuration.CsvJobName;

        var batch = new Batch() {
            Name = _fileReader.RemoveInvalidPathCharacters(jobName),
            Parts = parts
        };

        var response = await _bus.Send(new WriteTokensToCSV.Command(batch, outputDir));

        response.Match(
            file => OnFileGenerated?.Invoke(file),
            error => OnError?.Invoke($"{error.Title} - {error.Details}")
        );

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
