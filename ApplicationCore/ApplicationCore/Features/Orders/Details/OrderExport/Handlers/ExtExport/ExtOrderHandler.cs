using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport.Contracts;
using ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport.Services;
using ApplicationCore.Features.Shared.Services;
using ApplicationCore.Features.Companies.Contracts;

namespace ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport;

internal class ExtOrderHandler {

    private readonly IFileReader _fileReader;
    private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerByIdAsync;

    public ExtOrderHandler(IFileReader fileReader, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync) {
        _fileReader = fileReader;
        _getCustomerByIdAsync = getCustomerByIdAsync;
    }

    public async Task<string?> Handle(Order order, string jobName, string outputDirectory) {

        var products = order.Products
                            .Where(p => p is IPPProductContainer)
                            .Cast<IPPProductContainer>()
                            .SelectMany(c => c.GetPPProducts())
                            .ToList();


        jobName = jobName.Replace(".", "");
        if (jobName.Length > 30) jobName = jobName[..30];

        var customer = await _getCustomerByIdAsync(order.CustomerId);
        var customerName = customer?.Name ?? "";

        var job = new PPJob(jobName, order.OrderDate, customerName, products);

        var filePath = Path.Combine(outputDirectory, $"{_fileReader.RemoveInvalidPathCharacters(jobName)}.ext");

        var writer = new ExtWriter();

        new PPJobConverter(writer).ConvertOrder(job);

        writer.WriteFile(filePath);

        string errors = "";
        int index = 0;
        foreach (var product in products) {
            index++;
            foreach (var (key, value) in product.ManualOverrideParameters) {

                errors += $"[cab:{index}] {key} ==>> {value}<br>";

            }
        }

        return filePath;

    }

}