using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Updates;

internal class ApplicationVersionService {

	private readonly ILogger<ApplicationVersionService> _logger;
	private readonly HttpClient _httpClient;

	public ApplicationVersionService(ILogger<ApplicationVersionService> logger, IHttpClientFactory clientFactory) {
		_logger = logger;
		_httpClient = clientFactory.CreateClient();
	}

	public Task<string> DownloadInstaller(string uri) => throw new NotImplementedException();

	public Task<Version> GetCurrentVersion(string uri) => throw new NotImplementedException();

	public Task<Version> GetLatestVersion(string uri) => throw new NotImplementedException();

	public async Task<string> GetReleaseNotes(string uri) {

		Uri baseUri;
        if (uri.EndsWith('/')) baseUri = new(uri);
		else baseUri = new(uri + '/');

        Uri releaseNoteUri = new(baseUri, "release_notes.txt");

		_logger.LogInformation("Requesting release notes at URI {URI}", releaseNoteUri);

        var releaseNotes =  await _httpClient.GetStringAsync(releaseNoteUri);

		return releaseNotes;

	}

}
