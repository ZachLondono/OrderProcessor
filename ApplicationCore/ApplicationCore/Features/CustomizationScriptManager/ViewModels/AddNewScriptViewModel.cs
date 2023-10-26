using ApplicationCore.Features.CustomizationScriptList.Queries;
using ApplicationCore.Features.CustomizationScripts.Commands;
using ApplicationCore.Features.CustomizationScripts.Models;
using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.CustomizationScripts.Models;
using ApplicationCore.Shared.Services;

namespace ApplicationCore.Features.CustomizationScripts.ViewModels;

public class AddNewScriptViewModel {

    private readonly IBus _bus;
    private readonly IFilePicker _filePicker;
    private string _baseDirectory = "C:/";
    private Guid _orderId = Guid.Empty;

    public Action? OnPropertyChanged { get; set; }
    public Func<Task>? CloseView { get; set; }

    private string _scriptName = string.Empty;
    public string ScriptName {
        get => _scriptName;
        set {
            _scriptName = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private string _filePath = string.Empty;
    public string FilePath {
        get => _filePath;
        set {
            _filePath = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private bool _isFileBeingPicked = false;
    public bool IsFileBeingPicked {
        get => _isFileBeingPicked;
        set {
            _isFileBeingPicked = value;
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

    public AddNewScriptViewModel(IBus bus, IFilePicker filePicker) {
        _bus = bus;
        _filePicker = filePicker;
    }

    public async Task Loaded(Guid orderId) {

        try {

            _orderId = orderId;
            var result = await _bus.Send(new GetOrderWorkingDirectory.Query(orderId));
            result.OnSuccess(wd => _baseDirectory = wd);

        } catch (Exception ex) {

            Error = $"Failed to initialize - {ex.Message}";

        }

    }

    public void PickFile() {

        IsFileBeingPicked = true;
        _filePicker.PickFile(new() {
            Title = "Pick Script File",
            Filter = new("CSharp Script File", ".csx"),
            InitialDirectory = _baseDirectory
        }, file => {
            FilePath = file;
            IsFileBeingPicked = false;
        }, () => {
            IsFileBeingPicked = false;
        });

    }

    public async Task AddScript() {

        try {

            Error = null;

            await _bus.Send(new AddCustomizationScript.Command(new() {
                Id = Guid.NewGuid(),
                OrderId = _orderId,
                Name = ScriptName,
                FilePath = FilePath,
                Type = CustomizationType.DoweledDrawerBox
            }));

            if (CloseView is not null) {
                await CloseView();
            }

        } catch (Exception ex) {

            Error = $"Failed to save new script - {ex.Message}";

        }

    }

    public void OpenScriptFile() => throw new NotImplementedException();

}
