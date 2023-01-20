using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Details;

public interface IOrderDetailsViewModel {

    public Task<string> GetCompanyName(Guid companyId);

    public Task OpenCompanyPage(Guid companyId);

    public Task<ReleaseProfile?> GetVendorReleaseProfile(Guid vendorId);

    public Task<string> ExportOrderForPSI(Order order);

}
