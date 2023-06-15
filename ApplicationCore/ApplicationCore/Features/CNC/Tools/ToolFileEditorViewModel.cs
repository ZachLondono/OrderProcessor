using ApplicationCore.Features.CNC.Tools.Domain;
using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Settings;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace ApplicationCore.Features.CNC.Tools;

internal class ToolFileEditorViewModel {

    private readonly IBus _bus;
    private readonly string _filePath;

    private List<MachineToolMap> _toolMaps = new();
    public List<MachineToolMap> ToolMaps {
        get => _toolMaps;
        set {
            _toolMaps = value;
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

    public ToolFileEditorViewModel(IBus bus, IOptions<ConfigurationFiles> fileOptions) {
        _bus = bus;
        _filePath = fileOptions.Value.ToolConfigFile;
    }

    public async Task LoadToolFile() {

        try {

            var result = await _bus.Send(new GetTools.Query(_filePath));

            result.Match(
               tools => ToolMaps = new(tools),
               error => Error = error);

        } catch (Exception ex) {

            Error = new() {
                Title = "Failed to Load Data",
                Details = $"Exception thrown while trying to retrieve tool data from file : {ex.Message}"
            };

        }

    }

    public async Task SaveToolFile() {

        try {
            var result = await _bus.Send(new SetTools.Command(_filePath, ToolMaps));

            result.OnError(
                error => Error = error);

        } catch (Exception ex) {

            Error = new() {
                Title = "Failed to Save Changes",
                Details = $"Exception thrown while trying to save changes : {ex.Message}"
            };

        }

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
