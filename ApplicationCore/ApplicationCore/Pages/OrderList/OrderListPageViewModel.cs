using ApplicationCore.Features.Companies.Customers.List;
using ApplicationCore.Features.Companies.Vendors.List;
using ApplicationCore.Features.Orders.Details.Queries;
using ApplicationCore.Features.Orders.List;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Pages.OrderList;

public class OrderListPageViewModel {

    public string? SearchTerm { get; set; } = null;
    public Guid VendorId { get; set; } = Guid.Empty;
    public Guid CustomerId { get; set; } = Guid.Empty;
    public List<VendorListItem> Vendors { get; set; } = new();
    public List<CustomerListItem> Customers { get; set; } = new();
    public HashSet<OrderListItem> SelectedOrders { get; set; } = new();

    private readonly IBus _bus;

    public OrderListPageViewModel(IBus bus) {
        _bus = bus;
    }

    public async Task LoadCompanies() {

        var vendorsResponse = await _bus.Send(new GetAllVendors.Query());
        var customersResponse = await _bus.Send(new GetAllCustomers.Query());

        vendorsResponse.OnSuccess(Vendors.AddRange);
        customersResponse.OnSuccess(Customers.AddRange);

    }

    public async Task<List<Order>> GetSelectedOrdersAsync() {

        List<Order> orders = new();

        foreach (var item in SelectedOrders) {
            var response = await _bus.Send(new GetOrderById.Query(item.Id));
            response.OnSuccess(orders.Add);
        }

        return orders;

    }

}
