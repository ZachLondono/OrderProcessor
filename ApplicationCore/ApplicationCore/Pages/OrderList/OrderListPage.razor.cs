using ApplicationCore.Features.Orders.List;
using ApplicationCore.Widgets.Orders.OrderList;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;

namespace ApplicationCore.Pages.OrderList;

public partial class OrderListPage {

    public void OnSearchTermChange(ChangeEventArgs args) {

        if (args.Value is string val) {
            DataContext.SearchTerm = val;
        } else {
            DataContext.SearchTerm = null;
        }

    }

    protected override async Task OnInitializedAsync() {
        await DataContext.LoadCompanies();
    }

    protected override void OnAfterRender(bool firstRender) {
        if (firstRender && _orderList is not null) {
            _orderList.SelectedOrdersChanged += OnSelectedOrdersChanged;
        }
        base.OnAfterRender(firstRender);
    }

    public void OnSelectedOrdersChanged(HashSet<OrderListItem> selectedOrders) {
        DataContext.SelectedOrders = selectedOrders;
        StateHasChanged();
    }

    [CascadingParameter]
    public IModalService Modal { get; set; } = default!;

    private OrderListWidget? _orderList;

}
