
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;

namespace ApplicationCore.Features.Orders.Details.OrderExport;

public partial class ExportSetupDialog {

    [Parameter]
    public Order? Order { get; set; }

    [CascadingParameter]
    private BlazoredModalInstance ModalInstance { get; set; } = default!;

    [CascadingParameter]
    public IModalService ModalService { get; set; } = default!;

    public ExportConfiguration? Configuration { get; set; } = new();
    private string? _errorMessage = null;

    protected override async Task OnInitializedAsync() {

        if (Order is null) {
            _errorMessage = "No order set";
            return;
        }

        var vendor = await GetVendorByIdAsync(Order.VendorId);
        if (vendor is null) return;
        Configuration = new() {

            FillDovetailOrder = vendor.ExportProfile.ExportDBOrder,
            FillMDFDoorOrder = vendor.ExportProfile.ExportMDFDoorOrder,
            GenerateEXT = vendor.ExportProfile.ExportExtFile,
            ExtJobName = $"{Order.Number} - {Order.Name}",
            OutputDirectory = Order.WorkingDirectory

        };

        StateHasChanged();

    }

    private async Task Cancel() {

        await ModalInstance.CancelAsync();

    }

    public async Task Export() {

        _errorMessage = null;

        if (Configuration is null) {
            _errorMessage = "Could not load release configuration";
            return;
        }

        var parameters = new ModalParameters() {
            { "Configuration",  Configuration }
        };

        var options = new ModalOptions() {
                HideHeader = true,
                HideCloseButton = true,
                DisableBackgroundCancel = true,
                Size = ModalSize.Large
            };

        var dialog = ModalService.Show<ExportProgressDialog>("Order Export Progress", parameters, options);

        _ = await dialog.Result;

        await ModalInstance.CloseAsync();

    }

}
