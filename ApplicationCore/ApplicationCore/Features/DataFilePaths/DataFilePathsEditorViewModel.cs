using ApplicationCore.Features.Shared.Settings;
using ApplicationCore.Infrastructure.Bus;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace ApplicationCore.Features.Configuration;

internal class DataFilePathsEditorViewModel {

    public Action? OnPropertyChanged { get; set; }

    private DataFilePaths? _configuration = null;
    public DataFilePaths? Configuration {
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
    private readonly string _filePath;

    public DataFilePathsEditorViewModel(IBus bus, IOptions<ConfigurationFiles> fileOptions) {
        _bus = bus;
        _filePath = fileOptions.Value.DataConfigFile;
    }

    public async Task LoadConfiguration() {
        var result = await _bus.Send(new GetDataFilePaths.Query(_filePath));
        result.Match(
            config => Configuration = config,
            error => ErrorMessage = $"{error.Title} - {error.Details}"
        );
    }

    public async Task SaveChanges() {
        if (Configuration is null) return;
        var result = await _bus.Send(new UpdateDataFilePaths.Command(_filePath, Configuration));
        result.OnError(error => ErrorMessage = $"{error.Title} - {error.Details}");
    }

    public void OpenFile() {
        try {

            var psi = new ProcessStartInfo {
                FileName = Path.GetFullPath(_filePath),
                UseShellExecute = true
            };
            Process.Start(psi);

        } catch (Exception ex) {
            Debug.WriteLine(ex);
        }
    }

}
