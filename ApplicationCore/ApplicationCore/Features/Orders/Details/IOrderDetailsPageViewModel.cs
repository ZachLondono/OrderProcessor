using ApplicationCore.Features.Companies.Contracts.ValueObjects;

namespace ApplicationCore.Features.Orders.Details;

public interface IOrderDetailsPageViewModel {

    public Task<string> GetVendorName(Guid companyId);

    public Task<string> GetCustomerName(Guid companyId);

    public Task<ReleaseProfile?> GetVendorReleaseProfile(Guid vendorId);

}
