using Microsoft.AspNetCore.Components;

namespace Domain.Services;

public class NavigationService {

    private readonly NavigationManager _navigationManager;

    public NavigationService(NavigationManager navigationManager) {
        _navigationManager = navigationManager;
    }

    public void NavigateToOrderPage(Guid orderId) => _navigationManager.NavigateTo($"/orders/details/{orderId}", true);

    public void NavigateToCustomerPage(Guid customerId) => _navigationManager.NavigateTo($"/customers/{customerId}", true);

    public void NavigateToVendorPage(Guid vendorId) => _navigationManager.NavigateTo($"/vendors/{vendorId}", true);

    public void NavigateToOrderListPage() => _navigationManager.NavigateTo($"/orders/list", true);

}
