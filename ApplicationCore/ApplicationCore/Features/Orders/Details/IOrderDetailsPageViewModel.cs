using ApplicationCore.Features.CNC.LabelDB.Contracts;
using ApplicationCore.Features.CNC.ReleasePDF;
using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;

namespace ApplicationCore.Features.Orders.Details;

public interface IOrderDetailsPageViewModel {

    public Task<string> GetCompanyName(Guid companyId);

    public Task<ReleaseProfile?> GetVendorReleaseProfile(Guid vendorId);

    public Task<string> ExportOrderForPSI(Order order);

    public Task<GenerateReleaseForSelectedJobs.ReleaseGenerationResult?> GenerateCNCReleasePDF(Order order, string selectedPath, IEnumerable<AvailableJob> selectedJobs);

}
