using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Shared.Components.ProgressModal;
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;

namespace ApplicationCore.Features.Orders.OrderRelease;

public partial class OrderReleaseModal {

    [Parameter]
    public IEnumerable<Guid> OrderIds { get; set; } = Enumerable.Empty<Guid>();

    [CascadingParameter]
    private BlazoredModalInstance ModalInstance { get; set; } = default!;

    [CascadingParameter]
    public IModalService ModalService { get; set; } = default!;

    private List<Order> _orders = new();
    
    private string? _errorMessage = null;
    public bool _isReleasing = false;

    protected override void OnInitialized() {
        DataContext.OnPropertyChanged += StateHasChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender) {

        if (!firstRender) return;

        _orders = await DataContext.LoadOrdersAsync(OrderIds);
        if (!_orders.Any()) return;
        await DataContext.LoadConfiguration(_orders);
        
    }

    private void RemoveAdditionalFile(string filePath) {
        DataContext.Configuration.AdditionalFilePaths.Remove(filePath);
        StateHasChanged();
    }

    private void ChooseAdditionalFile()
        => FilePicker.PickFiles(new() {
            Title = "Select Additional File to Attach",
            InitialDirectory = _orders.FirstOrDefault()?.WorkingDirectory ?? "C:/",
            Filter = new("PDF File", "pdf")
        }, (fileNames) => {
            DataContext.Configuration.AdditionalFilePaths.AddRange(fileNames);
            InvokeAsync(StateHasChanged);
        });

    private void RemoveCNCDataFile(string filePath) {
        DataContext.Configuration.CNCDataFilePaths.Remove(filePath);
        StateHasChanged();
    }

    private void ChooseCNCDataFile()
        => FilePicker.PickFiles(new() {
            Title = "Select CADCode WS Report File",
            InitialDirectory = @"Y:\CADCode\Reports",
            Filter = new("CADCode WS Report", "xml")
        }, (fileNames) => {
            DataContext.Configuration.CNCDataFilePaths.AddRange(fileNames);
            InvokeAsync(StateHasChanged);
        });

    private string TruncateString(string value) {

        if (value.Length < 50) return value;

        return $"...{value[(value.Length - 27)..]}";

    }

    private async Task Cancel() {
        await ModalInstance.CancelAsync();
    }

    private async Task ShowReleaseProgressModal() {

        _isReleasing = true;
        _errorMessage = null;

        StateHasChanged();

        if (!_orders.Any()) {
            _isReleasing = false;
            StateHasChanged();
            return;
        }

        var parameters = DataContext.CreateReleaseProgressModalParameters(_orders);

        var options = new ModalOptions() {
            HideHeader = true,
            HideCloseButton = true,
            DisableBackgroundCancel = true,
            Size = ModalSize.Large
        };

        var dialog = ModalService.Show<ProgressModal>("Order Release Progress", parameters, options);

        _ = await dialog.Result;

        await ModalInstance.CloseAsync();

        _isReleasing = false;
        StateHasChanged();

    }

}
