using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.State;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace ApplicationCore.Features.Orders.Details;

public partial class OrderDetails {

    [Parameter]
    public RenderFragment? ActionPanel { get; set; }

    [Parameter]
    public RenderFragment<IProduct>? ProductActionsColumn { get; set; }

    [CascadingParameter]
    public IModalService Modal { get; set; } = default!;

    public List<RoomModel> Rooms { get; set; } = new();

    private string _note = string.Empty;
    private DateTime? _dueDate = null;
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

        _dueDate = OrderState.Order.DueDate;

        SeparateRooms();

    }

    private void SeparateRooms() {
        if (OrderState.Order is null) return;
        Rooms = OrderState.Order
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

    private async Task AddDueDate() {
        _dueDate = DateTime.Today;
        OrderState.SetDueDate(_dueDate);
        await OrderState.SaveDueDate();
        StateHasChanged();
    }

    private async Task RemoveDueDate() {
        _dueDate = null;
        OrderState.SetDueDate(null);
        await OrderState.SaveDueDate();
        StateHasChanged();
    }

    private async Task OnDueDateChanged(ChangeEventArgs args) {
        string newDueDateStr = args.Value?.ToString() ?? "";

        if (DateTime.TryParse(newDueDateStr, out DateTime newDueDate)) {
            _dueDate = newDueDate;
            OrderState.SetDueDate(newDueDate);
            await OrderState.SaveDueDate();
        }
    }

    private void ToggleUnits() {
        _useInches = !_useInches;
    }

    private async Task SaveNoteAsync() {

        try {
            await OrderState.SaveNote();
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

    private async Task WorkingDirectoryChanged() {
        await OrderState.SaveWorkingDirectory();
        StateHasChanged();
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
