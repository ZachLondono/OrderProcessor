using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Orders.OrderExport.Handlers.DoorOrderExport;
using ApplicationCore.Features.Orders.OrderExport.Handlers.DovetailOrderExport;
using ApplicationCore.Features.Orders.OrderExport.Handlers.ExtExport;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Shared;
using ApplicationCore.Shared.Services;
using ApplicationCore.Infrastructure.Bus;
using Microsoft.Extensions.Options;
using CADCodeProxy.Machining;
using ApplicationCore.Features.Orders.Shared.Domain;
using Microsoft.Extensions.Logging;
using ApplicationCore.Shared.Settings;
using CADCodeProxy.CSV;
using ApplicationCore.Shared.CustomizationScripts.Models;
using ApplicationCore.Shared.CustomizationScripts;
using ApplicationCore.Features.Orders.Shared.Domain.Products.DrawerBoxes;

namespace ApplicationCore.Features.Orders.OrderExport;

// TODO: Merge ExportService and ExportWidgetViewModel
internal class ExportService {

    public Action<string>? OnProgressReport;
    public Action<string>? OnFileGenerated;
    public Action<string>? OnError;
    public Action<string>? OnActionComplete;

    private readonly ILogger<ExportService> _logger;
    private readonly IBus _bus;
    private readonly ExportSettings _options;
    private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerByIdAsync;
    private readonly IFileReader _fileReader;

    public ExportService(ILogger<ExportService> logger, IBus bus, IOptions<ExportSettings> options, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync, IFileReader fileReader) {
        _logger = logger;
        _bus = bus;
        _options = options.Value;
        _getCustomerByIdAsync = getCustomerByIdAsync;
        _fileReader = fileReader;
    }

    public async Task Export(Order order, ExportConfiguration configuration) {

        if (configuration.OutputDirectory is null) {

            OnError?.Invoke("No export output directory is set");
            _logger.LogError("Export cancelled because no configuration does not have a output directory set {ExportConfiguration}", configuration);

            return;
        }

        try {

            var customerName = await GetCustomerName(order.CustomerId);
            var outputDir = ReplaceTokensInDirectory(customerName, configuration.OutputDirectory);

            await Task.WhenAll(new Task[] {
                GenerateMDFOrders(configuration, order, outputDir),
                GenerateDovetailOrders(configuration, order, outputDir),
                GenerateEXT(configuration, order, _options.EXTOutputDirectory),
                GenerateCSV(configuration, order, customerName, _options.CSVOutputDirectory),
            });

            OnActionComplete?.Invoke("Export Complete");

        } catch (Exception ex) {

            OnError?.Invoke($"One or more export steps failed - {ex.Message}");
            _logger.LogError(ex, "Exception thrown while exporting order");

        }

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

        var response = await Task.Run(() => _bus.Send(new ExportDoorOrder.Command(order, _options.MDFDoorTemplateFilePath, outputDir)));

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

        var response = await Task.Run(() => _bus.Send(new ExportDovetailOrder.Command(order, _options.DovetailTemplateFilePath, outputDir)));

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

        var response = await Task.Run(() => _bus.Send(new ExportEXT.Command(order, jobName.Trim(), outputDir)));

        response.Match(
            result => {

                foreach (var product in result.RequiredManualParameters) {
                    foreach (var (key, value) in product.Parameters) {
                        OnProgressReport?.Invoke($"[{product.ProductSequenceNum}] {product.ProductName} | {key} ==>> {value}");
                    }
                }

                OnFileGenerated?.Invoke(result.EXTFilePath);

            },
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

        var cncPartContainers = order.Products
                                    .Where(p => p is ICNCPartContainer)
                                    .Cast<ICNCPartContainer>()
                                    .ToList();

        await Task.Run(() => UpdateDoweledDrawerBoxes(order.Id, cncPartContainers));

        Part[] parts;

        try {

            parts = cncPartContainers
                .SelectMany(p => p.GetCNCParts(customerName))
                .ToArray();

        } catch (Exception ex) {

            _logger.LogError(ex, "Exception thrown while trying to generate cnc parts for order");
            OnError?.Invoke("Failed to generate CNC parts for order");
            throw;

        }

        if (!parts.Any()) {
            OnError?.Invoke("No parts in order to write to CSV");
            return;
        }

        OnProgressReport?.Invoke("Generating CSV file");

        try {

            string jobName = string.IsNullOrWhiteSpace(configuration.CsvJobName) ? $"{order.Number} - {order.Name}" : configuration.CsvJobName;
            var batch = new Batch() {
                Name = _fileReader.RemoveInvalidPathCharacters(jobName),
                Parts = parts
            };

            var filePath = await Task.Run(() => new CSVTokenWriter().WriteBatchCSV(batch, outputDir));

            OnFileGenerated?.Invoke(filePath);

        } catch (Exception ex) {

            _logger.LogError(ex, "Exception thrown while trying to write CADCode CSV token file");
            OnError?.Invoke("Failed to generate CSV token file");

        }

    }

    private async Task UpdateDoweledDrawerBoxes(Guid orderId, List<ICNCPartContainer> cncPartContainers) {
        IEnumerable<CustomizationScript> customizationScripts = await GetCustomizationScripts(orderId);
        var doweledDbScript = GetDoweledDrawerBoxScriptService(customizationScripts);
        if (doweledDbScript is not null) {

            var indexes = cncPartContainers.Where(p => p is DoweledDrawerBoxProduct).Select(p => cncPartContainers.IndexOf(p)).ToList();
            foreach (var idx in indexes) {
                cncPartContainers[idx] = await doweledDbScript.RunScript((DoweledDrawerBoxProduct)cncPartContainers[idx]);
            }

        }
    }

    private async Task<IEnumerable<CustomizationScript>> GetCustomizationScripts(Guid orderId) {

        IEnumerable<CustomizationScript> customizationScripts = Enumerable.Empty<CustomizationScript>();

        try {

            var scriptsResult = await _bus.Send(new GetCustomizationScriptsByOrderId.Query(orderId));
    
            scriptsResult.Match(
                scripts => customizationScripts = scripts,
                error => OnError?.Invoke(error.Title));
    

        } catch (Exception ex) {

            _logger.LogError(ex, "Exception thrown while trying to load customization scripts");
            OnError?.Invoke("Failed to load CSV customization scripts");

        }

        return customizationScripts;

    }

    public ScriptService<DoweledDrawerBoxProduct, DoweledDrawerBoxProduct>? GetDoweledDrawerBoxScriptService(IEnumerable<CustomizationScript> scripts) {

        var doweledDBCustomizationScript = scripts.FirstOrDefault(script => script.Type is CustomizationType.DoweledDrawerBox);

        if (doweledDBCustomizationScript is null) return null;

        try {
        
            ScriptService<DoweledDrawerBoxProduct, DoweledDrawerBoxProduct> scriptService = new(doweledDBCustomizationScript.FilePath);
            scriptService.LoadScript(new Type[] { typeof(Part) });
            return scriptService;
        
        } catch (Exception ex) {
        
            _logger.LogError(ex, $"Exception thrown trying to create script service - {doweledDBCustomizationScript.Name}");
            OnError?.Invoke($"Failed to initialize script - {doweledDBCustomizationScript.Name}");
        
        }

        return null;

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
