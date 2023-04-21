using ApplicationCore.Features.Tools.Domain;
using ApplicationCore.Infrastructure.Bus;
using System.Diagnostics;

namespace ApplicationCore.Features.Tools;

internal class ToolFileEditorViewModel {

    public const string FILE_PATH = @"./Configuration/tools.json";

    private readonly IBus _bus;

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

    public ToolFileEditorViewModel(IBus bus) {
        _bus = bus;
    }   

    public async Task LoadToolFile() {

        try {

            var result = await _bus.Send(new GetTools.Query(FILE_PATH));

            result.Match(
               tools => ToolMaps = new(tools),
               error => Error = error);

        } catch (Exception ex) {

            Error = new() {
                Title = "Failed to Load Data",
                Details = $"Exception thrown while trying to retreive tool data from file : {ex.Message}"
            };

        }

    }

    public async Task SaveToolFile() {

        try {
            var result = await _bus.Send(new SetTools.Command(FILE_PATH, ToolMaps));

            result.OnError(
                error => Error = error);

        } catch (Exception ex) {

            Error = new() {
                Title = "Failed to Save Changes",
                Details = $"Exception thrown while trying to save changes : {ex.Message}"
            };

        }

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
