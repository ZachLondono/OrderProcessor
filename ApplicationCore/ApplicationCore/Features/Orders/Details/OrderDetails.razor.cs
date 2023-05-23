using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.WorkOrders;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using System.Diagnostics;
using static ApplicationCore.Features.Orders.Details.ProductTables.CabinetProductTable;
using static ApplicationCore.Features.Orders.Details.ProductTables.ClosetPartProductTable;
using static ApplicationCore.Features.Orders.Details.ProductTables.DovetailDrawerBoxProductTable;
using static ApplicationCore.Features.Orders.Details.ProductTables.MDFDoorProductTable;

namespace ApplicationCore.Features.Orders.Details;

public partial class OrderDetails {

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [CascadingParameter]
    public IModalService Modal { get; set; } = default!;

    public List<CabinetRowModel> Cabinets { get; set; } = new();
    public List<ClosetPartRowModel> ClosetParts { get; set; } = new();
    public List<DovetailDrawerBoxRowModel> DrawerBoxes { get; set; } = new();
    public List<MDFDoorRowModel> Doors { get; set; } = new();

    private string _note = string.Empty;
    private string _workingDirectory = string.Empty;
    private string? _customerName = null;
    private string? _vendorName = null;

    private bool _useInches = false;

    protected override async Task OnInitializedAsync() {

        if (OrderState.Order is null) return;

        var vendor = await GetVendorByIdAsync(OrderState.Order.VendorId);
        var customer = await GetCustomerByIdAsync(OrderState.Order.VendorId);

        _vendorName = vendor?.Name ?? "";
        _customerName = customer?.Name ?? "";

        _note = OrderState.Order.Note;
        _workingDirectory = OrderState.Order.WorkingDirectory;

        Cabinets = OrderState.Order
                            .Products
                            .OfType<Cabinet>()
                            .Select(cab => new CabinetRowModel(cab))
                            .ToList();

        ClosetParts = OrderState.Order
                                .Products
                                .OfType<ClosetPart>()
                                .Select(cp => new ClosetPartRowModel(cp))
                                .ToList();

        DrawerBoxes = OrderState.Order
                                .Products
                                .OfType<DovetailDrawerBoxProduct>()
                                .Select(db => new DovetailDrawerBoxRowModel(db))
                                .ToList();

        Doors = OrderState.Order
                        .Products
                        .OfType<MDFDoorProduct>()
                        .Select(door => new MDFDoorRowModel(door))
                        .ToList();

        await UpdateProductStatuses();

    }

    private async Task UpdateProductStatuses() {

        if (OrderState.Order is null) {
            return;
        }

        await UpdateProducts(Cabinets);
        await UpdateProducts(ClosetParts);
        await UpdateProducts(DrawerBoxes);
        await UpdateProducts(Doors);

        StateHasChanged();

    }

    private async Task UpdateProducts<T>(IEnumerable<ProductRowModel<T>> products) where T : IProduct {

        if (OrderState.Order is null) return;

        foreach (var product in products) {

            product.IsComplete = await IsProductComplete(OrderState.Order.Id, product.Product.Id);

        }

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

    public override void Handle(WorkOrdersUpdateNotification notification) {  

        InvokeAsync(UpdateProductStatuses);

    }

    public abstract class ProductRowModel<T> where T : IProduct {

        public T Product { get; init; }
        public bool IsComplete { get; set; } = false;

        public ProductRowModel(T product ) {
            Product = product;
        }

    }



}
