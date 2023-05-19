using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Shared.Components.ProgressModal;
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;

namespace ApplicationCore.Features.Orders.Details.OrderRelease;

public partial class OrderReleaseWidget {

    [CascadingParameter]
    private BlazoredModalInstance ModalInstance { get; set; } = default!;

    [CascadingParameter]
    public IModalService ModalService { get; set; } = default!;

    public ReleaseConfiguration? Configuration { get; set; } = null;
    private string? _errorMessage = null;

    private bool _isReportLoadingFiles = true;

    protected override async Task OnInitializedAsync() {

        var order = OrderState.Order;
        if (order is null) {
            return;
        }

        await LoadConfiguration(order);
        await LoadCNCReportFiles(order);

    }

    private async Task LoadConfiguration(Order order) {

        var vendor = await GetVendorByIdAsync(order.VendorId);
        if (vendor is null) {
            return;
        }

        string releaseDirectory = order.WorkingDirectory;
        string invoiceDirectory = order.WorkingDirectory;

        Configuration = new() {

                SendReleaseEmail = vendor.ReleaseProfile.SendReleaseEmail,
                ReleaseEmailRecipients = vendor.ReleaseProfile.ReleaseEmailRecipients,
                GenerateJobSummary = vendor.ReleaseProfile.GenerateJobSummary,
                IncludeProductTablesInSummary = false,
                IncludeSuppliesInSummary = false,
                GeneratePackingList = vendor.ReleaseProfile.GeneratePackingList,
                IncludeInvoiceInRelease = vendor.ReleaseProfile.IncludeInvoice,
                ReleaseFileName = $"{order.Number} CUTLIST",
                ReleaseOutputDirectory = releaseDirectory,
                GenerateCNCRelease = false,

                GenerateInvoice = vendor.ReleaseProfile.GenerateInvoice,
                SendInvoiceEmail = vendor.ReleaseProfile.SendInvoiceEmail,
                InvoiceEmailRecipients = vendor.ReleaseProfile.InvoiceEmailRecipients,
                InvoiceFileName = $"{order.Number} INVOICE",
                InvoiceOutputDirectory = invoiceDirectory,

                EmailSenderName = vendor.EmailSender.Name,
                EmailSenderEmail = vendor.EmailSender.Email,
                EmailSenderPassword = vendor.EmailSender.ProtectedPassword

            };

        var customer = await GetCustomerByIdAsync(order.CustomerId);
        if (customer is not null && customer.BillingContact.Email is string invoiceEmail) {

            if (!string.IsNullOrEmpty(Configuration.InvoiceEmailRecipients))
                Configuration.InvoiceEmailRecipients = invoiceEmail + ";" + Configuration.InvoiceEmailRecipients;
            else Configuration.InvoiceEmailRecipients = invoiceEmail;

        }
        StateHasChanged();
    }

    private async Task LoadCNCReportFiles(Order order) {
        if (Configuration is null) return;
        var files = await GetReportFiles(order.Number);
        Configuration.CNCDataFilePaths.AddRange(files);
        Configuration.GenerateCNCRelease = Configuration.CNCDataFilePaths.Any();
        _isReportLoadingFiles = false;
        StateHasChanged();
    }

    private async Task<string[]> GetReportFiles(string number) {
        return await Task.Run(() => {
            try {
                return Directory.GetFiles(@"Y:\CADCode\Reports\", $"{number}* - *.xml");
            } catch {
                return Array.Empty<string>();
            }
        });
    }

    private void RemoveCNCDataFile(string filePath) {
        Configuration?.CNCDataFilePaths.Remove(filePath);
        StateHasChanged();
    }

    private void ChooseCNCDataFile()
        => FilePicker.PickFiles(new() {
            Title = "Select CADCode WS Report File",
            InitialDirectory = @"Y:\CADCode\Reports", 
            Filter = new("CADCode WS Report", "xml")
        }, (fileNames) => {
            Configuration?.CNCDataFilePaths.AddRange(fileNames);
            InvokeAsync(StateHasChanged);
        });

    private string TruncateString(string value) {

        if (value.Length < 50) return value;

        return $"...{value[(value.Length - 27)..]}";

    }

    private async Task Cancel() {

        await ModalInstance.CancelAsync();

    }

    private async Task Release() {

        _errorMessage = null;

        if (Configuration is null) {
            _errorMessage = "Could not load release configuration";
            return;
        }

        if (OrderState.Order is null) return;

        var parameters = new ModalParameters() {
            { "ActionRunner",  new ReleaseActionRunner(Service, OrderState.Order, Configuration) }
        };

        var options = new ModalOptions() {
                HideHeader = true,
                HideCloseButton = true,
                DisableBackgroundCancel = true,
                Size = ModalSize.Large
            };

        var dialog = ModalService.Show<ProgressModal>("Order Release Progress", parameters, options);

        _ = await dialog.Result;

        await ModalInstance.CloseAsync();
    }

    class ReleaseActionRunner : IActionRunner {

        public Action<ProgressLogMessage>? PublishProgressMessage { get; set; }

        private readonly ReleaseService _service;
        private readonly Order _order;
        private readonly ReleaseConfiguration _configuration;

        public ReleaseActionRunner(ReleaseService service, Order order, ReleaseConfiguration configuration) {
            _service = service;
            _order = order;
            _configuration = configuration;

            _service.OnProgressReport += (msg) => PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, msg));
            _service.OnError += (msg) => PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, msg));
            _service.OnActionComplete += (msg) => PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Success, msg));
            _service.OnFileGenerated += (msg) => PublishProgressMessage?.Invoke(new(ProgressLogMessageType.FileCreated, msg));
        }

        public async Task Run() {
            await _service.Release(_order, _configuration);
        }
    }

}
