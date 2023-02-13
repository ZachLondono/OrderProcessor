using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.ProductPlanner.Contracts;
using ApplicationCore.Features.ProductPlanner;
using ApplicationCore.Infrastructure;
using ApplicationCore.Features.Shared;

namespace ApplicationCore.Features.Orders.Details.OrderExport.Handlers;

internal class ExtOrderHandler {

    private readonly IFileReader _fileReader;
    private readonly IBus _bus;

    public ExtOrderHandler(IFileReader fileReader, IBus bus) {
        _fileReader = fileReader;
        _bus = bus;
    }

    public async Task Handle(Order order, string outputDirectory) {

        var products = order.Products
                            .Where(p => p is IPPProductContainer)
                            .Cast<IPPProductContainer>()
                            .SelectMany(c => c.GetPPProducts())
            .ToList();


        string jobName = $"{order.Number} - {order.Name}".Replace(".", "");
        if (jobName.Length > 30) jobName = jobName[..30];

        var job = new PPJob(jobName, order.OrderDate, products);

        var filePath = _fileReader.GetAvailableFileName(outputDirectory, jobName, "ext");

        var result = await _bus.Send(new GenerateEXTFile.Command(job, filePath));

        string errors = "";
        int index = 0;
        foreach (var product in products) {
            index++;
            foreach (var (key, value) in product.ManualOverrideParameters) {

                errors += $"[cab:{index}] {key} ==>> {value}<br>";

            }
        }

    }

}