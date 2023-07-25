using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using RestSharp;
using System.Net;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.LoadClosetProWebOrderData;

internal class ClosetProClient {

    private readonly string _username;
    private readonly string _password;
    private readonly string _instanceName;
    private readonly ILogger<ClosetProClient> _logger;

    private WebAppState _viewState = new("", "", "");

    public Action<string>? OnError { get; set; }

    public ClosetProClient(string username, string password, string instanceName, ILogger<ClosetProClient> logger) {
        _username = username;
        _password = password;
        _instanceName = instanceName;
        _logger = logger;
    }

    public async Task<string?> GetCutListDataAsync(int designId) {

        var client = CreateClient();

        _viewState = await GetWebAppStateAsync(client);

        var loginRequest = CreateLoginRequest();
        var loginResponse = await client.ExecuteAsync(loginRequest);

        if (loginResponse.StatusCode == HttpStatusCode.Redirect) {

            var cutListRequest = CreateGenerateCutListRequest(designId);
            var stdCLResponse = await client.ExecuteAsync(cutListRequest);

            if (stdCLResponse.StatusCode == HttpStatusCode.Redirect) {

                string? location = stdCLResponse?.Headers?.FirstOrDefault(h => h.Name == "Location")?.Value?.ToString() ?? null;
                if (location is null) {
                    OnError?.Invoke("Could not generate cut list");
                    _logger.LogError("Location header was not found in redirect response when requesting to generate cut list - {Response}", stdCLResponse);
                    return null;
                }

                var downloadRequest = CreateDownloadRequest(designId);
                var downloadResponse = await client.ExecuteAsync(downloadRequest);

                if (!downloadResponse.IsSuccessful) {
                    OnError?.Invoke("Downloading cut list was unsuccessful");
                    _logger.LogError("Server returned unsuccessful status code when attempting to download cut list - {Response}", downloadResponse);
                    return null;
                } else if (downloadResponse.ContentType != "text/csv") {
                    OnError?.Invoke("Unexpected response when attempting to download cut list");
                    _logger.LogError("Server returned unexpected content type when attempting to download standard cut list - {Response}", downloadResponse);
                    return null;
                }

                return downloadResponse.Content;

            } else {

                OnError?.Invoke("Unexpected response when attempting to generate cut list");
                _logger.LogError("Unexpected response from server when attempting to generate standard cut list - {Response}", stdCLResponse);
                return null;

            }

        } else {

            OnError?.Invoke("Unexpected response when attempting to log in");
            _logger.LogError("Unexpected response from server when attempting to authenticate - {Response}", loginResponse);
            return null;

        }

    }

    private RestClient CreateClient() {

        var cookieJar = new CookieContainer();
        var options = new RestClientOptions($"https://{_instanceName}.closetprosoftware.com") {
            MaxTimeout = -1,
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36",
            FollowRedirects = false,
            CookieContainer = cookieJar
        };

        return new(options);

    }

    private RestRequest CreateDownloadRequest(int designId) {

        var request = new RestRequest($"/StandardCutList.aspx?PID={designId}&CID=0&T=1&P=", Method.Get);
        AddCPHeaders(request);
        request.AddParameter("__EVENTVALIDATION", _viewState.EventValidation);
        request.AddParameter("__VIEWSTATE", _viewState.ViewState);
        request.AddParameter("__VIEWSTATEGENERATOR", _viewState.ViewStateGenerator);

        return request;

    }

    private RestRequest CreateGenerateCutListRequest(int designId) {

        var request = new RestRequest("GenerateCutList.aspx", Method.Get);
        AddCPHeaders(request);

        request.AddUrlSegment("T", "1");    // Type?
        request.AddUrlSegment("PID", designId);

        request.AddParameter("__EVENTVALIDATION", _viewState.EventValidation);
        request.AddParameter("__VIEWSTATE", _viewState.ViewState);
        request.AddParameter("__VIEWSTATEGENERATOR", _viewState.ViewStateGenerator);

        return request;

    }

    private RestRequest CreateLoginRequest() {

        var request = new RestRequest("/login.aspx", Method.Post);
        AddCPHeaders(request);

        request.AddParameter("UserEmail", _username);
        request.AddParameter("UserPW", _password);
        request.AddParameter("__EVENTVALIDATION", _viewState.EventValidation);
        request.AddParameter("__VIEWSTATE", _viewState.ViewState);
        request.AddParameter("__VIEWSTATEGENERATOR", _viewState.ViewStateGenerator);
        request.AddParameter("btnSubmit", "Login");

        request.AddParameter("RegEmail", "");
        request.AddParameter("RegFName", "");
        request.AddParameter("RegLName", "");
        request.AddParameter("RegPhone", "");
        request.AddParameter("RegUserPW", "");
        request.AddParameter("RegUserPW2", "");
        request.AddParameter("__EVENTARGUMENT", "");
        request.AddParameter("__EVENTTARGET", "");
        request.AddParameter("__LASTFOCUS", "");
        request.AddParameter("g-recaptcha-response", "");

        return request;

    }

    public void AddCPHeaders(RestRequest request) {
        request.AddHeader("host", $"{_instanceName}.closetprosoftware.com");
        request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
        request.AddHeader("sec-ch-ua", "\"Not.A/Brand\";v=\"8\", \"Chromium\";v=\"114\", \"Google Chrome\";v=\"114\"");
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request.AddHeader("Sec-Fetch-Dest", "document");
        request.AddHeader("Sec-Fetch-Mode", "navigate");
        request.AddHeader("Sec-Fetch-Site", "same-origin");
        request.AddHeader("Sec-Fetch-User", "?1");
        request.AddHeader("sec-ch-ua-mobile", "?0");
        request.AddHeader("sec-ch-ua-platform", "\"Windows\"");
        request.AddHeader("Upgrade-Insecure-Requests", "1");
    }

    private static async Task<WebAppState> GetWebAppStateAsync(RestClient client) {

        var request = new RestRequest("", Method.Get);
        RestResponse response = await client.ExecuteAsync(request);

        var doc = new HtmlDocument();
        doc.LoadHtml(response.Content);

        var viewState = doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "");
        var viewStateGen = doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", "");
        var eventValid = doc.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "");

        return new(viewState, viewStateGen, eventValid);

    }

    private record WebAppState(string ViewState, string ViewStateGenerator, string EventValidation);

}
