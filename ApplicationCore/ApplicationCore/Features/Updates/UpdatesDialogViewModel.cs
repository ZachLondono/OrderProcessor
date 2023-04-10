using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Updates;

internal class UpdatesDialogViewModel {

    private const string RELEASE_INSTALLER_URI = "http://zacharylondono.com/release";
    private const string PREVIEW_INSTALLER_URI = "http://zacharylondono.com/preview";

    /// <summary>
    /// Uri to the directory where the app installer is located
    /// </summary>
    private string InstallerUri { get => UsePreviewChannel ? PREVIEW_INSTALLER_URI : RELEASE_INSTALLER_URI; }

    public bool UsePreviewChannel { get; private set; } = true; // TODO: get preview flag from config file

    public Version CurrentVersion { get; private set; } = new("0.0.0.0");

    private Version? _availableUpdate = null;
    public Version? AvailableUpdate {
        get => _availableUpdate;
        private set {
            _availableUpdate = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private string _latestReleaseNotes = string.Empty;
    public string LatestReleaseNotes {
        get => _latestReleaseNotes;
        private set {
            _latestReleaseNotes = value;
            OnPropertyChanged?.Invoke();
        }
    }

    /// <summary>
    /// True if the newest update was downloaded and ready to be installed
    /// </summary>
    public bool IsUpdateReady {
        get => _updateInstallerFilePath is not null && !IsUpdateDownloading;
    }

    private string? _updateInstallerFilePath = null;

    private bool _isUpdateDownloading = false;
    public bool IsUpdateDownloading {
        get => _isUpdateDownloading;
        private set {
            _isUpdateDownloading = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private bool _isCheckingForUpdates = false;
    public bool IsCheckingForUpdates {
        get => _isCheckingForUpdates;
        private set {
            _isCheckingForUpdates = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private string? _error = null;
    public string? Error {
        get => _error;
        private set {
            _error = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public Action? OnPropertyChanged { get; set; }

    private readonly ApplicationVersionService _versionService;
    private readonly ILogger<UpdatesDialogViewModel> _logger;

    public UpdatesDialogViewModel(ApplicationVersionService versionService, ILogger<UpdatesDialogViewModel> logger) {
        _versionService = versionService;
        _logger = logger;
    }

    public async Task CheckForUpdates() {
        IsCheckingForUpdates = true;
        try {
            LatestReleaseNotes = await _versionService.GetReleaseNotes(InstallerUri);
            CurrentVersion = await _versionService.GetCurrentVersion(InstallerUri);
            var newVersion = await _versionService.GetLatestVersion(InstallerUri);
            _logger.LogInformation("Latest version available is {Version}", newVersion);
            _availableUpdate = newVersion == CurrentVersion ? null : newVersion;
		} catch (Exception ex) {
            Error = "Could not check if any updates are available";
            _logger.LogError(ex, "Exception thrown while checking for updates");
        }
		IsCheckingForUpdates = false;
    }

    public async Task DownloadUpdate() {
        IsUpdateDownloading = true;
        try { 
            _updateInstallerFilePath = await _versionService.DownloadInstaller(InstallerUri);
            _logger.LogInformation("Update installer downloaded to {FilePath}", _updateInstallerFilePath);
        } catch (Exception ex) {
            Error = "Could not download new update";
            _logger.LogError(ex, "Exception thrown while downloading update installer");
        }
		IsUpdateDownloading = false;
    }

    public void InstallUpdate() {

    }

}
