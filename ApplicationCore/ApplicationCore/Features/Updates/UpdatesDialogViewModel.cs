using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Updates;

internal class UpdatesDialogViewModel {

    private const string RELEASE_INSTALLER_URI = "http://zacharylondono.com/orders/release";
    private const string PREVIEW_INSTALLER_URI = "http://zacharylondono.com/orders/preview";

    /// <summary>
    /// Uri to the directory where the app installer is located
    /// </summary>
    private string InstallerUri { get => UsePreviewChannel ? PREVIEW_INSTALLER_URI : RELEASE_INSTALLER_URI; }

    public bool UsePreviewChannel { get; private set; } = true; // TODO: get preview flag from config file

    public Version CurrentVersion { get; private set; } = new("0.0.0.0");

    private bool _isUpdateAvailable = false;
    public bool IsUpdateAvailable {
        get => _isUpdateAvailable;
        private set {
            _isUpdateAvailable = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private bool _isInstallingUpdate = false;
    public bool IsInstallingUpdate {
        get => _isInstallingUpdate;
        private set {
            _isInstallingUpdate = value;
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

    public async Task Initialize() {
        try {
            LatestReleaseNotes = await _versionService.GetReleaseNotes(InstallerUri);
            CurrentVersion = ApplicationVersionService.GetCurrentVersion();
            var channel = await ApplicationVersionService.GetReleaseChannel();
            UsePreviewChannel = channel != "release";
        } catch {

        }
    }

    public async Task CheckForUpdates() {
        IsCheckingForUpdates = true;
        try {
            IsUpdateAvailable = await ApplicationVersionService.IsUpdateAvailable();
		} catch (Exception ex) {
            Error = "Could not check if any updates are available";
            _logger.LogError(ex, "Exception thrown while checking for updates");
        }
		IsCheckingForUpdates = false;
    }

    public async Task DownloadUpdate() {
        IsInstallingUpdate = true;
        try { 
            await ApplicationVersionService.InstallLatestVersion(InstallerUri);
        } catch (Exception ex) {
            Error = "Could not download new update";
            _logger.LogError(ex, "Exception thrown while downloading update installer");
        }
        IsInstallingUpdate = false;
    }

    public async Task InstallUpdate() {
        IsInstallingUpdate = true;
        try {
            await ApplicationVersionService.InstallLatestVersion(InstallerUri);
            await CheckForUpdates();
        } catch (Exception ex) {
            Error = "Could not install new update";
            _logger.LogError(ex, "Exception thrown while installing update");
        }
        IsInstallingUpdate = false;
    }

}
