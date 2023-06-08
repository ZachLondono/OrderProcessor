using ApplicationCore.Infrastructure.Bus;
using System.Diagnostics;

namespace ApplicationCore.Features.Configuration;

internal class ConfigurationEditorViewModel {

    public const string FILE_PATH = @"./Configuration/data.json";

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
        var result = await _bus.Send(new GetConfiguration.Query(FILE_PATH));
        result.Match(
            config => Configuration = config,
            error => ErrorMessage = $"{error.Title} - {error.Details}"
        );
    }

    public async Task SaveChanges() {
        if (Configuration is null) return;
        var result = await _bus.Send(new UpdateConfiguration.Command(FILE_PATH, Configuration));
        result.OnError(error => ErrorMessage = $"{error.Title} - {error.Details}");
    }

    public static void OpenFile() {
        try {

            var psi = new ProcessStartInfo {
                FileName = Path.GetFullPath(FILE_PATH),
                UseShellExecute = true
            };
            Process.Start(psi);

        } catch (Exception ex) {
            Debug.WriteLine(ex);
        }
    }

}
