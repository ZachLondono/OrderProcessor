
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Shared.Components.ProgressModal;
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
            { "ActionRunner",  new ExportActionRunner(Service, Configuration) }
        };

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

    class ExportActionRunner : IActionRunner {

        private readonly ExportService _exportService;
        private readonly ExportConfiguration _configuration;

        public ExportActionRunner(ExportService exportService, ExportConfiguration configuration) {
            _exportService = exportService;
            _configuration = configuration;
            _exportService.OnProgressReport += (msg) => PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, msg));
            _exportService.OnError += (msg) => PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, msg));
            _exportService.OnFileGenerated += (msg) => PublishProgressMessage?.Invoke(new(ProgressLogMessageType.FileCreated, msg));
            _exportService.OnActionComplete += (msg) => PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Success, msg));
        }

        public Action<ProgressLogMessage>? PublishProgressMessage { get; set; }

        public async Task Run() => await _exportService.Export(_configuration);

    }

}
