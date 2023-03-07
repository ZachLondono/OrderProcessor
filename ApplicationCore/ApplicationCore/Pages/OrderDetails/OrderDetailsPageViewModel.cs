using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Companies.Contracts.ValueObjects;
using ApplicationCore.Features.Orders.Details;

namespace ApplicationCore.Pages.OrderDetails;

internal class OrderDetailsPageViewModel : IOrderDetailsPageViewModel {

    public Action? OnPropertyChanged { get; set; }

    private readonly CompanyDirectory.GetVendorByIdAsync _getVendorById;
    private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerById;

    public OrderDetailsPageViewModel(CompanyDirectory.GetVendorByIdAsync getVendorById, CompanyDirectory.GetCustomerByIdAsync getCustomerById) {
        _getVendorById = getVendorById;
        _getCustomerById = getCustomerById;
    }

    public async Task<string> GetVendorName(Guid vendorId) {

        var vendor = await _getVendorById(vendorId);

        return vendor?.Name ?? "";

    }

    public async Task<string> GetCustomerName(Guid vendorId) {

        var customer = await _getCustomerById(vendorId);

        return customer?.Name ?? "";

    }

    public async Task<ReleaseProfile?> GetVendorReleaseProfile(Guid vendorId) {

        var vendor = await _getVendorById(vendorId);

        return vendor?.ReleaseProfile;

    }

}
