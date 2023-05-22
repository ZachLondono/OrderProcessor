using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Shared.Components.ProgressModal;
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;

namespace ApplicationCore.Features.Orders.OrderRelease;

public partial class OrderReleaseModal {

    [Parameter]
    public Order? Order { get; set; } = null;

    [CascadingParameter]
    private BlazoredModalInstance ModalInstance { get; set; } = default!;

    [CascadingParameter]
    public IModalService ModalService { get; set; } = default!;

    private string? _errorMessage = null;

    protected override async Task OnInitializedAsync() {

        DataContext.OnPropertyChanged += StateHasChanged;

        if (Order is null) return;

        await DataContext.LoadConfiguration(Order);

    }

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

        if (Order is null) return;

        var parameters = DataContext.CreateReleaseProgressModalParameters(Order);

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
