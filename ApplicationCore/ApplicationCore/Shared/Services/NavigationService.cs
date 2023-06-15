using ApplicationCore.Features.Orders.Shared.State;
using Microsoft.AspNetCore.Components;

namespace ApplicationCore.Shared.Services;

public class NavigationService {

    private readonly OrderState _orderState;
    private readonly NavigationManager _navigationManager;

    public NavigationService(OrderState orderState, NavigationManager navigationManager) {
        _orderState = orderState;
        _navigationManager = navigationManager;
    }

    public async Task NavigateToOrderPage(Guid orderId) {
        await _orderState.LoadOrder(orderId);
        _navigationManager.NavigateTo("/orders/details", true);
    }

    public void NavigateToCustomerPage(Guid customerId) => _navigationManager.NavigateTo($"/customers/{customerId}", true);

    public void NavigateToVendorPage(Guid vendorId) => _navigationManager.NavigateTo($"/vendors/{vendorId}", true);

    public void NavigateToOrderListPage() => _navigationManager.NavigateTo($"/orders/list", true);

}
