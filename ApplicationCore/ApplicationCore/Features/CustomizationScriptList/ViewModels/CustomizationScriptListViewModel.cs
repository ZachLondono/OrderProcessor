using ApplicationCore.Features.CustomizationScripts.Commands;
using ApplicationCore.Features.CustomizationScripts.Models;
using ApplicationCore.Features.CustomizationScripts.Queries;
using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Features.CustomizationScripts.ViewModels;

public class CustomizationScriptListViewModel {

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

    public CustomizationScriptListViewModel(IBus bus) {
        _bus = bus;
    }

    public async Task Loaded(Guid orderId) {

        try {

            Error = null;

            var response = await _bus.Send(new GetCustomizationScripts.Query(orderId));

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

            response.OnError(error => Error = $"{error.Title} - {error.Details}");


        } catch (Exception ex) {

            Error = $"Failed to delete script - {ex.Message}";

        }

    }

    public Task AddScript() => throw new NotImplementedException();

}
