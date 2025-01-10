using ApplicationCore.Features.OrderList;
using Companies.Customers.List;
using Companies.Vendors.List;
using Domain.Infrastructure.Bus;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Pages.OrderList;

public class OrderListPageViewModel {

    public string? SearchTerm { get; set; } = null;
    public Guid VendorId { get; set; } = Guid.Empty;
    public Guid CustomerId { get; set; } = Guid.Empty;
    public List<VendorListItem> Vendors { get; set; } = new();
    public List<CustomerListItem> Customers { get; set; } = new();
    public HashSet<OrderListItem> SelectedOrders { get; set; } = new();

    private readonly IBus _bus;
    private ILogger<OrderListPageViewModel> _logger;

    public OrderListPageViewModel(IBus bus, ILogger<OrderListPageViewModel> logger) {
        _bus = bus;
        _logger = logger;
    }

    public async Task LoadCompanies() {

        try {

            var vendorsResponse = await _bus.Send(new GetAllVendors.Query());
            var customersResponse = await _bus.Send(new GetAllCustomers.Query());

            vendorsResponse.OnSuccess(Vendors.AddRange);
            customersResponse.OnSuccess(Customers.AddRange);

        } catch (Exception ex) {

            _logger.LogError(ex, "Exception thrown while trying to load company information");

        }

    }

}
