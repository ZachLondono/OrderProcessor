using ApplicationCore.Features.Orders.Details.Commands;
using ApplicationCore.Features.Orders.Details.Models;
using ApplicationCore.Features.Orders.Details.Queries;
using Domain.Orders.Entities;
using Blazored.Modal;
using Blazored.Modal.Services;
using Domain.Orders.Entities.Products;
using Microsoft.AspNetCore.Components;
using System.Diagnostics;
using ApplicationCore.Features.Orders.AddProductToOrder;

namespace ApplicationCore.Features.Orders.Details.Views;

public partial class OrderDetails {

    [Parameter]
    public Guid OrderId { get; set; }

    [Parameter]
    public RenderFragment<Order>? ActionPanel { get; set; }

    [Parameter]
    public RenderFragment<IProduct>? ProductActionsColumn { get; set; }

    [CascadingParameter]
    public IModalService Modal { get; set; } = default!;

    public List<RoomModel> Rooms { get; set; } = new();

    private Order? _order = null;
    private bool _isLoading = true;
    private bool _isNoteDirty = false;
    private string _note = string.Empty;
    private bool _useInches = false;

    protected override async Task OnParametersSetAsync() {

        var result = await Bus.Send(new GetOrderById.Query(OrderId));

        result.Match(
            order => {

                _order = order;
                _note = order.Note;
                SeparateRooms();
                _isLoading = false;

            },
            _ => _isLoading = false);

    }

    private void SeparateRooms() {
        if (_order is null) return;
        Rooms = _order
                    .Products
                    .GroupBy(p => p.Room)
                    .Select(RoomModel.FromGrouping)
                    .ToList();
    }

    public void OpenCustomerPage(Guid companyId) {
        NavigationService.NavigateToCustomerPage(companyId);
    }

    public void OpenVendorPage(Guid companyId) {
        NavigationService.NavigateToVendorPage(companyId);
    }

    private async Task OpenAdditionalItemModal(AdditionalItem item) {

        if (_order is null) return;

        var modal = Modal.Show<AdditionalItemModal>("Item Editor", new ModalParameters() {
            { "Item", item }
        });

        var result = await modal.Result;

        if (result.Confirmed) {

            // Force a refresh
            NavigationService.NavigateToOrderPage(_order.Id);

            if (result.Data is AdditionalItem) {
                // TODO: update order model and call state has changed, rather than refreshing page
            }
        }


    }

    private void ToggleUnits() {
        _useInches = !_useInches;
    }

    private async Task AddNewProduct() {

        var instance = Modal.Show<AddProductForm>(
            "Add Product",
            new ModalParameters() {
                { "OrderId", OrderId }
            });

        _ = await instance.Result;

    }

    private async Task SaveNoteAsync() {

        if (_order is null) return;

        try {

            var result = await Bus.Send(new UpdateOrderNote.Command(_order.Id, _note));
            result.OnSuccess(_ => _isNoteDirty = false);

        } catch (Exception ex) {
            Debug.WriteLine("Exception thrown while saving changes");
            Debug.WriteLine(ex);
        }

        StateHasChanged();

    }

    private void OnNoteChanged(ChangeEventArgs args) {
        if (_order is null) return;
        _note = args.Value?.ToString() ?? "";
        _order.Note = _note;
        _isNoteDirty = true;
    }

    public abstract class ProductRowModel<T> where T : IProduct {

        public T Product { get; init; }
        public bool IsComplete { get; set; } = false;

        public ProductRowModel(T product) {
            Product = product;
        }

    }

    public void OnProductsChanged() {
        SeparateRooms();
        StateHasChanged();
    }

}
