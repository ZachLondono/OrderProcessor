using ApplicationCore.Features.Orders.Shared.State;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;

namespace ApplicationCore.Shared.Services;

public class NavigationService {

    private readonly OrderState _orderState;
    private readonly NavigationManager _navigationManager;
    private readonly IModalService _modalService;

	public NavigationService(OrderState orderState, NavigationManager navigationManager, IModalService modalService) {
		_orderState = orderState;
		_navigationManager = navigationManager;
		_modalService = modalService;
	}

	public async Task NavigateToOrderPage(Guid orderId) {
        if (await _orderState.LoadOrder(orderId)) {
            _navigationManager.NavigateTo("/orders/details", true);
        } else {
            await _modalService.OpenErrorDialog(new() {
                Title = "Navigation Failed",
                Details = "Failed to navigate to requested order page."
            });
        }
    }

    public void NavigateToCustomerPage(Guid customerId) => _navigationManager.NavigateTo($"/customers/{customerId}", true);

    public void NavigateToVendorPage(Guid vendorId) => _navigationManager.NavigateTo($"/vendors/{vendorId}", true);

    public void NavigateToOrderListPage() => _navigationManager.NavigateTo($"/orders/list", true);

}
