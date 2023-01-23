using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.Companies;
using ApplicationCore.Features.Orders.List;
using ApplicationCore.Infrastructure;
using Microsoft.AspNetCore.Components;

namespace ApplicationCore.Pages.OrderList;

internal class OrderListPageViewModel : IOrderListViewModel {

    private readonly CompanyState _companyState;
    private readonly IBus _bus;
    private readonly NavigationManager _navigationManager;

    public OrderListPageViewModel(CompanyState companyState, IBus bus, NavigationManager navigationManager) {
        _companyState = companyState;
        _bus = bus;
        _navigationManager = navigationManager;
    }

    public async Task<string> GetCompanyName(Guid companyId) {

        Response<string?> response = await _bus.Send<string?>(new GetCompanyNameById.Query(companyId));

        string name = string.Empty;

        response.OnSuccess(
            companyName => name = companyName ?? string.Empty
        );

        return name;

    }

    public async Task OpenCompanyPage(Guid companyId) {
        await _companyState.LoadCompany(companyId);
        _navigationManager.NavigateTo("/companies/details", true);
    }

}
