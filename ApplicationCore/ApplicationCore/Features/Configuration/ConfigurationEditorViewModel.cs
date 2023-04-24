using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Features.Configuration;

internal class ConfigurationEditorViewModel {

    public Action? OnPropertyChanged { get; set; }

    private AppConfiguration? _configuration = null;
    public AppConfiguration? Configuration {
        get => _configuration;
        private set {
            _configuration = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private string? _error = null;
    public string? ErrorMessage {
        get => _error;
        set {
            _error = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private readonly IBus _bus;

    public ConfigurationEditorViewModel(IBus bus) {
        _bus = bus;
    }

    public async Task LoadConfiguration() {
        var result = await _bus.Send(new GetConfiguration.Query());
        result.Match(
            config => Configuration = config,
            error => ErrorMessage = $"{error.Title} - {error.Details}"
        );
    }

    public async Task SaveChanges() {
        if (Configuration is null) return;
        var result = await _bus.Send(new UpdateConfiguration.Command(Configuration));
        result.OnError(error => ErrorMessage = $"{error.Title} - {error.Details}");
    }

}
