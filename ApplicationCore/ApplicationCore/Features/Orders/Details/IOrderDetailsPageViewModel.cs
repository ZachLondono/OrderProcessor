using ApplicationCore.Features.Companies.Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Details;

public interface IOrderDetailsPageViewModel {

    public Task<string> GetCompanyName(Guid companyId);

    public Task<ReleaseProfile?> GetVendorReleaseProfile(Guid vendorId);

}
