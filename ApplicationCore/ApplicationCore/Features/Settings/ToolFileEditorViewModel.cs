using ApplicationCore.Shared.Settings.Tools;
using Domain.Infrastructure.Bus;
using Domain.Infrastructure.Settings;

namespace ApplicationCore.Features.Settings;

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
        try {
            Configuration = options.Value;
            Error = null;
        } catch {
            Error = new() {
                Title = "Error",
                Details = "Settings could not be loaded"
            };
        }
    }

    public void SaveToolFile() {

        try {

            _options.Update(config => {
                config.MachineToolMaps = CleanNames(Configuration.MachineToolMaps);
            });

            Error = null;

        } catch {

            Error = new() {
                Title = "Error",
                Details = "Changes could not be saved"
            };

        }

    }

    private static List<MachineToolMap> CleanNames(List<MachineToolMap> maps)
       => maps.Select(m => {
                  m.MachineName = m.MachineName.Trim();
                  m.Tools = m.Tools
                             .Select(t => new Tool() {
                                 Position = t.Position,
                                 Name = t.Name.Trim(),
                                 AlternativeNames = t.AlternativeNames.Select(t => t.Trim()).ToList()
                             })
                             .ToList();
                  return m;
              })
              .ToList();

}
