using Blazored.Modal;
using ApplicationCore.Features.Orders.Shared.State;
using ApplicationCore.Features.Orders.WorkingDirectory;
using ApplicationCore.Infrastructure.Bus;
using Microsoft.AspNetCore.Components;
using Blazored.Modal.Services;
using ApplicationCore.Features.Orders.Details.Models;

namespace ApplicationCore.Features.Orders.Details.Views;

public partial class ChangeWorkingDirectoryModal {

    [Parameter]
    public Guid? OrderId { get; set; }

    [CascadingParameter]
    private BlazoredModalInstance BlazoredModal { get; set; } = default!;

    private bool _isLoading = true;
    private WorkingDirectoryEditModel _model = new();
    private Error? _error = null;

    protected override async Task OnInitializedAsync() {

        if (OrderId is null) {
            _error = new() {
                Title = "No Order Found",
                Details = "There is no order set to update."
            };
            return;
        }

        try {

            var result = await Bus.Send(new GetOrderWorkingDirectory.Query((Guid)OrderId));

            result.Match(
                directory => {
                    _model.OriginalDirectory = directory;
                    _model.NewDirectory = directory;
                    _error = null;
                    _isLoading = false;
                },
                error => _error = error);

        } catch (Exception ex) {

            _error = new() {
                Title = "Error occurred while loading order working directory",
                Details = ex.Message
            };

        }

    }

    private async Task UpdateWorkingDirectory() {

        if (OrderId is null) {
            return;
        }

        bool wasMigrateSuccessful = true;
        if (_model.Mode != MigrationType.None) {

            var migrationResult = await Bus.Send(new MigrateWorkingDirectory.Command(_model.OriginalDirectory, _model.NewDirectory, _model.Mode));
            migrationResult.OnError(error => {
                _error = error;
                wasMigrateSuccessful = false;
            });

        }

        var updateResult = await Bus.Send(new UpdateOrderWorkingDirectory.Command((Guid)OrderId, _model.NewDirectory));
        await updateResult.MatchAsync(
            async _ => {
                if (!wasMigrateSuccessful) return;
                await BlazoredModal.CloseAsync(ModalResult.Ok(_model.NewDirectory));
            },
            error => {
                _error = error;
                return Task.CompletedTask;
            });

    }

    private Task Cancel() => BlazoredModal.CancelAsync();

}