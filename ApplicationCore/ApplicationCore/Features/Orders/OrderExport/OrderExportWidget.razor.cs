
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Shared.Components.ProgressModal;
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Orders.OrderExport;

public partial class OrderExportWidget {

    [Parameter]
    public Order? Order { get; set; } = null;

    [CascadingParameter]
    private BlazoredModalInstance ModalInstance { get; set; } = default!;

    [CascadingParameter]
    public IModalService ModalService { get; set; } = default!;

    private string? _errorMessage = null;

    protected override async Task OnInitializedAsync() {

        if (Order is null) {
            _errorMessage = "No order set";
            StateHasChanged();
            return;
        }

        try {
            await DataContext.LoadConfiguration(Order);
        } catch (Exception ex) {
            _errorMessage = "Error occurred while trying to load configuration";
            Logger.LogError(ex, "Exception thrown while trying to load export configuration for order {OrderId}", Order.Id);
        }
        
        StateHasChanged();

    }

    private async Task Cancel() {

        await ModalInstance.CancelAsync();

    }

    public async Task ShowExportProgressModal() {

        var parameters = DataContext.CreateExportProgressModalParameters();

        var options = new ModalOptions() {
                HideHeader = true,
                HideCloseButton = true,
                DisableBackgroundCancel = true,
                Size = ModalSize.Large
            };

        var dialog = ModalService.Show<ProgressModal>("Order Export Progress", parameters, options);

        // Wait for the progress dialog to close
        _ = await dialog.Result;

        // Close the setup modal 
        await ModalInstance.CloseAsync();

    }

}
