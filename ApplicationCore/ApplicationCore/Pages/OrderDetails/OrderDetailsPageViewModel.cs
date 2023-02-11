using ApplicationCore.Features.CNC.LabelDB.Contracts;
using ApplicationCore.Features.CNC.ReleasePDF;
using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.ProductPlanner.Contracts;
using ApplicationCore.Features.ProductPlanner;
using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Details;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Shared;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Pages.OrderDetails;

internal class OrderDetailsPageViewModel : IOrderDetailsPageViewModel {

    public Action? OnPropertyChanged { get; set; }

    private readonly IBus _bus;
    private readonly IFileReader _fileReader;

    public OrderDetailsPageViewModel(IBus bus, IFileReader fileReader) {
        _bus = bus;
        _fileReader = fileReader;
    }

    public async Task<string> GetCompanyName(Guid companyId) {

        var result = await _bus.Send(new GetCompanyNameById.Query(companyId));

        string name = string.Empty;

        result.OnSuccess(companyName => name = companyName ?? string.Empty);

        return name;

    }

    public async Task<ReleaseProfile?> GetVendorReleaseProfile(Guid vendorId) {

        var response = await _bus.Send(new GetCompanyById.Query(vendorId));

        ReleaseProfile? profile = null;

        response.OnSuccess(vendor => {
            profile = vendor?.ReleapseProfile ?? null;
        });

        return profile;
    }

    public async Task<string> ExportOrderForPSI(Order order) {

        string errors = string.Empty;

        await Task.Run(async () => {

            var products = order
                            .Products
                            .Where(p => p is IPPProductContainer)
                            .Cast<IPPProductContainer>()
                            .SelectMany(c => c.GetPPProducts())
            .ToList();


            string jobName = $"{order.Number} - {order.Name}".Replace(".", "");

            if (jobName.Length > 30) jobName = jobName[..30];

            var job = new PPJob(jobName, order.OrderDate, products);

            var filePath = _fileReader.GetAvailableFileName(@"C:\CP3\CPDATA", jobName, "ext");

            var result = await _bus.Send(new GenerateEXTFile.Command(job, filePath));

            int index = 0;
            foreach (var product in products) {
                index++;
                foreach (var (key, value) in product.ManualOverrideParameters) {

                    errors += $"[cab:{index}] {key} ==>> {value}<br>";

                }
            }

        });

        return errors;

    }

    public async Task<GenerateReleaseForSelectedJobs.ReleaseGenerationResult?> GenerateCNCReleasePDF(Order order, string selectedPath, IEnumerable<AvailableJob> selectedJobs) {

        var response = await _bus.Send(new GenerateReleaseForSelectedJobs.Command(order.Id, "Title", "Customer Name", "Vendor Name", DateTime.Now, selectedPath, selectedJobs));

        GenerateReleaseForSelectedJobs.ReleaseGenerationResult? result = null;

        response.OnSuccess(r => result = r);

        return result;

    }

}
