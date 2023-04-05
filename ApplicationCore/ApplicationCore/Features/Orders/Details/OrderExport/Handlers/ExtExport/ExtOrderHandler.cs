using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport.Contracts;
using ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport.Services;
using ApplicationCore.Features.Shared.Services;

namespace ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport;

internal class ExtOrderHandler {

    private readonly IFileReader _fileReader;

    public ExtOrderHandler(IFileReader fileReader) {
        _fileReader = fileReader;
    }

    public Task<string?> Handle(Order order, string outputDirectory) {

        var products = order.Products
                            .Where(p => p is IPPProductContainer)
                            .Cast<IPPProductContainer>()
                            .SelectMany(c => c.GetPPProducts())
                            .ToList();


        string jobName = $"{order.Number} - {order.Name}".Replace(".", "");
        if (jobName.Length > 30) jobName = jobName[..30];

        var job = new PPJob(jobName, order.OrderDate, products);

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

        return Task.FromResult<string?>(filePath);

    }

}