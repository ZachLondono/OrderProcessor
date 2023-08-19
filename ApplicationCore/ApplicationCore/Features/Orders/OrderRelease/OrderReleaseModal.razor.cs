using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Shared.Components.ProgressModal;
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;

namespace ApplicationCore.Features.Orders.OrderRelease;

public partial class OrderReleaseModal {

    [Parameter]
    public List<Order> Orders { get; set; } = new();

    [CascadingParameter]
    private BlazoredModalInstance ModalInstance { get; set; } = default!;

    [CascadingParameter]
    public IModalService ModalService { get; set; } = default!;

    private string? _errorMessage = null;

    protected override async Task OnInitializedAsync() {

        DataContext.OnPropertyChanged += StateHasChanged;

        if (!Orders.Any()) return;

        await DataContext.LoadConfiguration(Orders);

    }
    
    private void RemoveAdditionalFile(string filePath) {
        DataContext.Configuration.AdditionalFilePaths.Remove(filePath);
        StateHasChanged();
    }

    private void ChooseAdditionalFile()
        => FilePicker.PickFiles(new() {
            Title = "Select Additional File to Attach",
            InitialDirectory = Orders.FirstOrDefault()?.WorkingDirectory ?? "C:/",
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

        _errorMessage = null;

        if (!Orders.Any()) return;

        var parameters = DataContext.CreateReleaseProgressModalParameters(Orders);

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

}
