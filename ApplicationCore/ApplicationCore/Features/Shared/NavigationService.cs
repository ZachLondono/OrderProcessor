using ApplicationCore.Features.Companies;
using ApplicationCore.Features.Orders.Shared.State;
using Microsoft.AspNetCore.Components;

namespace ApplicationCore.Features.Shared;

public class NavigationService {

    private readonly OrderState _orderState;
    private readonly CompanyState _companyState;
    private readonly NavigationManager _navigationManager;

    public NavigationService(OrderState orderState, CompanyState companyState, NavigationManager navigationManager) {
        _orderState = orderState;
        _companyState = companyState;
        _navigationManager = navigationManager;
    }

    public async Task NavigateToOrderPage(Guid orderId) {
        await _orderState.LoadOrder(orderId);
        _navigationManager.NavigateTo("/orders/details", true);
    }

    public async Task NavigateToCompanyPage(Guid companyId) {
        await _companyState.LoadCompany(companyId);
        _navigationManager.NavigateTo("/companies/details", true);
    }

}
