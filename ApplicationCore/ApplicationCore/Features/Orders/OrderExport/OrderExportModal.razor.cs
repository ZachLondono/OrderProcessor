using Domain.Orders.Entities;
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Domain.Components.ProgressModal;
using Domain.Infrastructure.Bus;
using ApplicationCore.Features.Orders.Details.Queries;

namespace ApplicationCore.Features.Orders.OrderExport;

public partial class OrderExportModal {

    [Parameter]
    public Guid OrderId { get; set; }

    [CascadingParameter]
    private BlazoredModalInstance ModalInstance { get; set; } = default!;

    [CascadingParameter]
    public IModalService ModalService { get; set; } = default!;

    [Inject]
    public IBus Bus { get; set; } = default!;

    private Order? _order = null;
    private string? _errorMessage = null;

    protected override async Task OnAfterRenderAsync(bool firstRender) {

        if (!firstRender) return;

        var response = await Bus.Send(new GetOrderById.Query(OrderId));
        response.OnSuccess(o => _order = o);

        if (_order is null) {
            _errorMessage = "No order set found";
            StateHasChanged();
            return;
        }

        try {
            await DataContext.LoadConfiguration(_order);
        } catch (Exception ex) {
            _errorMessage = "Error occurred while trying to load configuration";
            Logger.LogError(ex, "Exception thrown while trying to load export configuration for order {OrderId}", _order.Id);
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
