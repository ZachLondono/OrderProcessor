using Microsoft.AspNetCore.Components;
using ApplicationCore.Shared;
using Blazored.Modal;
using Blazored.Modal.Services;
using System.Diagnostics;
using ApplicationCore.Features.Orders.Shared.State;

namespace ApplicationCore.Features.Orders.Details.Views;

public partial class WorkingDirectoryWidget {

    [Parameter]
    public Guid OrderId { get; set; }

    [CascadingParameter]
    public IModalService Modal { get; set; } = default!;

    private bool _doesDirectoryExist = false;
    private bool _isLoading = true;
    private string _workingDirectory = string.Empty;

    protected override async Task OnInitializedAsync() {

        var result = await Bus.Send(new GetOrderWorkingDirectory.Query(OrderId));

        result.OnSuccess(directory => {
            _workingDirectory = directory;
            _doesDirectoryExist = Directory.Exists(_workingDirectory);
            _isLoading = false;
        });

    }

    public async Task UpdateWorkingDirectory() {

        var modal = Modal.Show<ChangeWorkingDirectoryModal>(
            "Working Directory",
            new ModalParameters() { { "OrderId", OrderId } },
            new ModalOptions() {
                DisableBackgroundCancel = true,
                AnimationType = ModalAnimationType.FadeInOut,
                Size = ModalSize.Large
            });

        var result = await modal.Result;
        if (result.Confirmed && result.Data is string newDirectory) {
            _workingDirectory = newDirectory;
            _doesDirectoryExist = Directory.Exists(_workingDirectory);
            StateHasChanged();
        }

    }

    public void OpenWorkingDirectory() {

        if (Directory.Exists(_workingDirectory)) {

            ProcessStartInfo startInfo = new ProcessStartInfo {
                Arguments = _workingDirectory.Replace('/', '\\'),
                FileName = "explorer.exe"
            };

            Process.Start(startInfo);

        } else {
            Modal.OpenErrorDialog(new() { Title = "Cannot Open Working Directory", Details = "The working directory doesn't exist or cannot be accessed" });
        }

    }

}