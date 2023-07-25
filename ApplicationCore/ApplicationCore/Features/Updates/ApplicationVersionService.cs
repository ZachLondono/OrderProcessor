using Microsoft.Extensions.Logging;
using System.Reflection;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace ApplicationCore.Features.Updates;

internal class ApplicationVersionService {

    private readonly ILogger<ApplicationVersionService> _logger;
    private readonly HttpClient _httpClient;

    public ApplicationVersionService(ILogger<ApplicationVersionService> logger, IHttpClientFactory clientFactory) {
        _logger = logger;
        _httpClient = clientFactory.CreateClient();
    }

    public static async Task InstallLatestVersion(string uri) {

        var package = Package.Current;
        Uri? installerUri = package.GetAppInstallerInfo()?.Uri;
        if (installerUri == null) {
            Console.WriteLine("App installer info returned null"); // For some reason GetAppInstallerInfo can not be relied upon
            installerUri = new(new(uri), "Package.appinstaller"); // This URI needs to point to the .appinstaller file on the server
        } else {
            Console.WriteLine("Package correctly returned app installer info");
        }

        var availability = await GetUpdateAvailabilityAsync(0);
        switch (availability) {
            case PackageUpdateAvailability.Available:
            case PackageUpdateAvailability.Required:
                uint res = RelaunchHelper.RegisterApplicationRestart("", RelaunchHelper.RestartFlags.NONE);
                var packageManager = new PackageManager();
                var packageVolume = packageManager.GetDefaultPackageVolume();
                await packageManager.AddPackageByAppInstallerFileAsync(installerUri, AddPackageByAppInstallerOptions.ForceTargetAppShutdown, packageVolume);
                break;
            case PackageUpdateAvailability.NoUpdates:
            case PackageUpdateAvailability.Unknown:
            default:
                break;
        }

    }

    public static Version GetCurrentVersion() {
        var version = Package.Current.Id.Version;
        return new(version.Major, version.Minor, version.Build, version.Revision);
    }

    public static async Task<bool> IsUpdateAvailable() {
        var availability = await GetUpdateAvailabilityAsync(0);
        return (availability == PackageUpdateAvailability.Available) || (availability == PackageUpdateAvailability.Required);
    }

    public async Task<string> GetReleaseNotes(string uri) {

        Uri baseUri;
        if (uri.EndsWith('/')) baseUri = new(uri);
        else baseUri = new(uri + '/');

        Uri releaseNoteUri = new(baseUri, "release_notes.txt");

        _logger.LogInformation("Requesting release notes at URI {URI}", releaseNoteUri);

        var releaseNotes = await _httpClient.GetStringAsync(releaseNoteUri);

        return releaseNotes;

    }

    private static async Task<PackageUpdateAvailability> GetUpdateAvailabilityAsync(int attempt) {

        if (attempt > 3) throw new InvalidOperationException("Unable to check for update availability");

        var package = Package.Current;
        PackageUpdateAvailabilityResult result = await package.CheckUpdateAvailabilityAsync();
        if (result.ExtendedError is Exception exception) {
            Console.WriteLine(exception);
            return await GetUpdateAvailabilityAsync(attempt++);
        }

        return result.Availability;

    }

    public static async Task<string> GetReleaseChannel() {
        string exeFilePath = Assembly.GetExecutingAssembly().Location;
        string? workPath = Path.GetDirectoryName(exeFilePath);
        if (workPath is null) {
            return "unknown";
        }

        var filePath = Path.Combine(workPath, "channel.txt");
        if (!Path.Exists(filePath)) {
            return "unknown";
        }
        return await File.ReadAllTextAsync(filePath);

    }

}
