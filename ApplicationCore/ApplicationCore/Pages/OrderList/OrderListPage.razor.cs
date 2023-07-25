using ApplicationCore.Features.Orders.List;
using ApplicationCore.Features.Orders.OrderRelease;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.State;
using ApplicationCore.Infrastructure.Bus;
using Blazored.Modal;
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

    public void OnSelectedOrdersChanged(HashSet<OrderListItem> selectedOrders) {
        DataContext.SelectedOrders = selectedOrders;
        StateHasChanged();
    } 

    [CascadingParameter]
    public IModalService Modal { get; set; } = default!;

    [Inject]
    public IBus? Bus { get; set; }

    private async Task ReleaseSelectedOrders() {

        if (Bus is null) {
            return;
        }

        List<Order> orders = new();

        foreach (var item in DataContext.SelectedOrders) {
            var response = await Bus.Send(new GetOrderById.Query(item.Id));
            response.OnSuccess(orders.Add);
        }

        var dialog = Modal.Show<OrderReleaseModal>("Release Setup",
            new ModalParameters() {
                { "Orders", orders }
            }, new ModalOptions() {
                HideHeader = true,
                HideCloseButton = true,
                DisableBackgroundCancel = true,
                Size = ModalSize.Medium
            });

        _ = await dialog.Result;

    }

}
