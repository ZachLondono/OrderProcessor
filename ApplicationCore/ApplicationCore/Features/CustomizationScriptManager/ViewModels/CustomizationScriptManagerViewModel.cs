using ApplicationCore.Features.CustomizationScripts.Commands;
using ApplicationCore.Features.CustomizationScripts.Models;
using ApplicationCore.Features.CustomizationScripts.Queries;
using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.CustomizationScripts;
using ApplicationCore.Shared.CustomizationScripts.Models;
using Blazored.Modal;
using Blazored.Modal.Services;

namespace ApplicationCore.Features.CustomizationScripts.ViewModels;

public class CustomizationScriptManagerViewModel {

    private readonly IBus _bus;

    public Action? OnPropertyChanged { get; set; }

    private List<CustomizationScript> _scripts = new();
    public List<CustomizationScript> Scripts {
        get => _scripts;
        set {
            _scripts = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private string? _error = null;
    public string? Error {
        get => _error;
        set {
            _error = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public CustomizationScriptManagerViewModel(IBus bus) {
        _bus = bus;
    }

    public async Task Loaded(Guid orderId) {

        try {

            Error = null;

            var response = await _bus.Send(new GetCustomizationScriptsByOrderId.Query(orderId));

            response.Match(
                scripts => Scripts = new(scripts),
                error => Error = $"{error.Title} - {error.Details}");

        } catch (Exception ex) {

            Error = $"Failed to load scripts - {ex.Message}";

        }

    }

    public async Task DeleteScript(Guid scriptId) {

        try {

            Error = null;

            var response = await _bus.Send(new DeleteCustomizationScript.Command(scriptId));

            response.Match(
                _ => {

                    var deletedScript = Scripts.First(s => s.Id == scriptId);
                    Scripts.Remove(deletedScript);
                    OnPropertyChanged?.Invoke();

                },
                error => Error = $"{error.Title} - {error.Details}");

        } catch (Exception ex) {

            Error = $"Failed to delete script - {ex.Message}";

        }

    }

    public async Task ShowAddScriptModal(IModalService modalService, Guid orderId) {

        var modal = modalService.Show<CustomizationScriptManager.Views.AddCustomizationScript>(
            "New Customization Script",
            new ModalParameters() {
                { "OrderId", orderId }
            },
            new ModalOptions() {
                DisableBackgroundCancel = true,
                HideHeader = true
            });

        var result = await modal.Result;

        if (result.Confirmed) {
            await Loaded(orderId);
        }

    }

    public Task ShowEditScriptModal(IModalService modalService, Guid scriptId) => throw new NotImplementedException();

}
