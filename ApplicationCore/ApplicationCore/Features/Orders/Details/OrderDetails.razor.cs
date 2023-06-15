using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.State;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace ApplicationCore.Features.Orders.Details;

public partial class OrderDetails {

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [CascadingParameter]
    public IModalService Modal { get; set; } = default!;

    public List<Room> Rooms { get; set; } = new();

    private string _note = string.Empty;
    private string _workingDirectory = string.Empty;
    private string? _customerName = null;
    private string? _vendorName = null;

    private bool _useInches = false;

    protected override async Task OnInitializedAsync() {

        if (OrderState.Order is null) return;

        var vendor = await GetVendorByIdAsync(OrderState.Order.VendorId);
        var customer = await GetCustomerByIdAsync(OrderState.Order.CustomerId);

        _vendorName = vendor?.Name ?? "";
        _customerName = customer?.Name ?? "";

        _note = OrderState.Order.Note;
        _workingDirectory = OrderState.Order.WorkingDirectory;

        Rooms = OrderState.Order
                    .Products
                    .GroupBy(p => p.Room)
                    .Select(Room.FromGrouping)
                    .ToList();

    }

    public void OpenCustomerPage(Guid companyId) {
        NavigationService.NavigateToCustomerPage(companyId);
    }

    public void OpenVendorPage(Guid companyId) {
        NavigationService.NavigateToVendorPage(companyId);
    }

    private void ToggleUnits() {
        _useInches = !_useInches;
    }

    private async Task SaveChangesAsync() {

        try {
            await OrderState.SaveChanges();
        } catch (Exception ex) {
            Debug.WriteLine("Exception thrown while saving changes");
            Debug.WriteLine(ex);
        }

        StateHasChanged();

    }

    private void OnNoteChanged(ChangeEventArgs args) {
        _note = args.Value?.ToString() ?? "";
        OrderState.SetNote(_note);
    }

    private void OnWorkingDirectoryChanged(ChangeEventArgs args) {
        _workingDirectory = args.Value?.ToString() ?? "";
        OrderState.SetWorkingDirectory(_workingDirectory);
    }

    private async Task SaveRoomNameChange(Room room) {

        if (!room.IsDirty) return;

        foreach (var product in room.Products) {

            try {
                product.Room = room.Name;
                await Bus.Send(new UpdateProduct.Command(product));
            } catch (Exception ex) {
                Debug.Write("Exception thrown while saving product room name change");
                Debug.WriteLine(ex);
            }

        }

        room.IsDirty = false;
        StateHasChanged();

    }

    private static void OnRoomNameChanged(ChangeEventArgs args, Room room) {
        room.Name = args.Value?.ToString() ?? "";
    }

    public abstract class ProductRowModel<T> where T : IProduct {

        public T Product { get; init; }
        public bool IsComplete { get; set; } = false;

        public ProductRowModel(T product) {
            Product = product;
        }

    }

}
