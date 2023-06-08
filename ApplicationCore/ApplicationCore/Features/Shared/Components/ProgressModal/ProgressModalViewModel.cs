using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ApplicationCore.Features.Shared.Components.ProgressModal;

public class ProgressModalViewModel {

    public Action? OnPropertyChanged { get; set; }

    public bool InProgress {
        get => _inProgress;
        set {
            _inProgress = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public bool IsComplete {
        get => _isComplete;
        set {
            _isComplete = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private bool _inProgress;
    private bool _isComplete;
    private readonly ILogger<ProgressModalViewModel> _logger;

    public ProgressModalViewModel(ILogger<ProgressModalViewModel> logger) {
        _logger = logger;
    }

    public async Task RunAction(IActionRunner actionRunner) {
        InProgress = true;
        await actionRunner.Run();
        IsComplete = true;
        InProgress = false;
    }

    public void OpenFile(string filePath) {

        try {

            var psi = new ProcessStartInfo {
                FileName = filePath,
                UseShellExecute = true
            };
            Process.Start(psi);

        } catch (Exception ex) {

            _logger.LogError(ex, "Exception thrown while trying to open file '{FilePath}'", filePath);

        }

    }

}
