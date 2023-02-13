using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.Orders.Details;
using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Pages.OrderDetails;

internal class OrderDetailsPageViewModel : IOrderDetailsPageViewModel {

    public Action? OnPropertyChanged { get; set; }

    private readonly IBus _bus;

    public OrderDetailsPageViewModel(IBus bus) {
        _bus = bus;
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

}
