using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Settings;
using ApplicationCore.Shared.Settings.Tools;

namespace ApplicationCore.Features.CNC.Tools;

internal class ToolFileEditorViewModel {

    private ToolConfiguration _configuration = new();
    public ToolConfiguration Configuration {
        get => _configuration;
        set {
            _configuration = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private Error? _error = null;
    public Error? Error {
        get => _error;
        set {
            _error = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public Action? OnPropertyChanged { get; set; }

    private readonly IWritableOptions<ToolConfiguration> _options;

    public ToolFileEditorViewModel(IWritableOptions<ToolConfiguration> options) {
        _options = options;
        Configuration = options.Value;
    }

    public void SaveToolFile() {
        _options.Update(config => {
            config.MachineToolMaps = Configuration.MachineToolMaps;
        });
    }

}
