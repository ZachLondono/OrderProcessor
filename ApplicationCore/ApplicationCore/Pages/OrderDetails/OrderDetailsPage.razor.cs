using ApplicationCore.Features.Orders.OrderExport;
using ApplicationCore.Features.Orders.OrderRelease;
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;

namespace ApplicationCore.Pages.OrderDetails;

public partial class OrderDetailsPage {

    [CascadingParameter]
    public IModalService Modal { get; set; } = default!;

    protected override void OnInitialized() {
        DataContext.OnPropertyChanged += StateHasChanged;
    }

    private bool _isExporting = false;
    private bool _isReleasing = false;

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

}
