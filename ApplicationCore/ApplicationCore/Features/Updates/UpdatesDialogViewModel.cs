namespace ApplicationCore.Features.Updates;

internal class UpdatesDialogViewModel {

    private const string RELEASE_INSTALLER_URI = "http://zacharylondono.com/release";
    private const string PREVIEW_INSTALLER_URI = "http://zacharylondono.com/preview";

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

    public Action? OnPropertyChanged { get; set; }

    private readonly ApplicationVersionService _versionService;

    public UpdatesDialogViewModel(ApplicationVersionService versionService) {
        _versionService = versionService;
    }

    public async Task CheckForUpdates() {
        IsCheckingForUpdates = true;
        CurrentVersion = await _versionService.GetCurrentVersion(InstallerUri);
        var newVersion = await _versionService.GetLatestVersion(InstallerUri);
        _availableUpdate = newVersion == CurrentVersion ? null : newVersion;
        IsCheckingForUpdates = false;
    }

    public async Task DownloadUpdate() {
        IsUpdateDownloading = true;
        _updateInstallerFilePath = await _versionService.DownloadInstaller(InstallerUri);
        IsUpdateDownloading = false;
    }

    public void InstallUpdate() {

    }

}
