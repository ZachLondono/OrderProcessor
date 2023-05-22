using ApplicationCore.Features.Companies.Contracts.ValueObjects;
using ApplicationCore.Features.Orders.OrderRelease;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Shared;
using ApplicationCore.Features.WorkOrders;
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using System.Diagnostics;
using static ApplicationCore.Features.Orders.Details.ProductTables.CabinetProductTable;
using static ApplicationCore.Features.Orders.Details.ProductTables.ClosetPartProductTable;
using static ApplicationCore.Features.Orders.Details.ProductTables.DovetailDrawerBoxProductTable;
using static ApplicationCore.Features.Orders.Details.ProductTables.MDFDoorProductTable;

namespace ApplicationCore.Features.Orders.Details;

public partial class OrderDetails {

    [CascadingParameter]
    public IModalService Modal { get; set; } = default!;

    [Parameter]
    public IOrderDetailsPageViewModel? ViewModel { get; set; }

    public List<CabinetRowModel> Cabinets { get; set; } = new();
    public List<ClosetPartRowModel> ClosetParts { get; set; } = new();
    public List<DovetailDrawerBoxRowModel> DrawerBoxes { get; set; } = new();
    public List<MDFDoorRowModel> Doors { get; set; } = new();

    private string _note = string.Empty;
    private string _workingDirectory = string.Empty;
    private string? _customerName = null;
    private string? _vendorName = null;
    private ReleaseProfile? _customReleaseProfile = null;

    private bool _useInches = false;

    private bool _isReleasing = false;
    private bool _isExporting = false;

    protected override async Task OnInitializedAsync() {

        if (OrderState.Order is null) return;

        if (ViewModel is not null) {
            _vendorName = await ViewModel.GetVendorName(OrderState.Order.VendorId);
            _customerName = await ViewModel.GetCustomerName(OrderState.Order.CustomerId);
        }

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

    private async Task LoadReleaseProfile() {

        if (OrderState.Order is null || _customReleaseProfile is not null) return;

        if (ViewModel is not null) {
            _customReleaseProfile = await ViewModel.GetVendorReleaseProfile(OrderState.Order.VendorId);
        }

        if (_customReleaseProfile is null) {
            _ = await Modal.OpenInformationDialog("Could not Load Release Settings", "", InformationDialog.MessageType.Warning);
        }

    }

    public void OpenCustomerPage(Guid companyId) {
        NavigationService.NavigateToCustomerPage(companyId);
    }

    public void OpenVendorPage(Guid companyId) {
        NavigationService.NavigateToVendorPage(companyId);
    }

    private async Task ReleaseOrder() {

        if (_isReleasing == true || OrderState.Order is null) {
            return;
        }

        _isReleasing = true;

        var dialog = Modal.Show<OrderReleaseWidget>("Release Setup",
            new ModalParameters() {
                { "Order", OrderState.Order }
            }, new ModalOptions() {
                HideHeader = true,
                HideCloseButton = true,
                DisableBackgroundCancel = true,
                Size = ModalSize.Medium
            });

        _ = await dialog.Result;

        _isReleasing = false;

    }

    private async Task ExportOrder() {

        if (_isExporting == true || OrderState.Order is null) {
            return;
        }

        _isExporting = true;

        var dialog = Modal.Show<OrderExportWidget>("Export Setup",
            new ModalParameters() {
                { "Order", OrderState.Order }
            }, new ModalOptions() {
                HideHeader = false,
                HideCloseButton = true,
                DisableBackgroundCancel = true,
                Size = ModalSize.Medium
            });

        _ = await dialog.Result;

        _isExporting = false;

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
