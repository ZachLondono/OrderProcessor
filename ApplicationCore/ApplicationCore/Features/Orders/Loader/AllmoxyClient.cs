using ApplicationCore.Features.Orders.Loader.Rest;
using RestSharp;

namespace ApplicationCore.Features.Orders.Loader;

internal class AllmoxyClient : IAllmoxyClient {

    private readonly IRestClient _client;
    private readonly string _instanceName;
    private readonly string _username;
    private readonly string _password;

    private const string NOT_LOGGED_IN_CONTENT_TYPE = "text/html";
    private const string EXPORT_CONTENT_TYPE = "application/xml";
    private const string LOG_IN_FORM_CONTENT_TYPE = "application/x-www-form-urlencoded";
    private const int MAX_RETRIES = 3;

    public AllmoxyClient(string instanceName, string username, string password, IRestClient client) {
        _instanceName = instanceName;
        _username = username;
        _password = password;

        _client = client;

    }

    public string GetExport(string orderNumber, int index) => GetExport(orderNumber, index, 0);

    private string GetExport(string orderNumber, int index, int tries) {

        if (tries > MAX_RETRIES) {
            throw new InvalidOperationException("Could not log in to Allmoxy");
        }

        var request = new RestRequest($"orders/export_partlist/{orderNumber}/{index}/", Method.Get);
        RestResponse response = _client.Execute(request);

        switch (response.ContentType) {
            case NOT_LOGGED_IN_CONTENT_TYPE:
                LogIn();
                return GetExport(orderNumber, index, ++tries);

            case EXPORT_CONTENT_TYPE:
                if (response.Content is null) {
                    throw new InvalidOperationException("No data returned");
                }
                return response.Content;

            default:
                throw new InvalidOperationException($"Unexpected response from server {response.StatusCode}");
        }

    }

    private void LogIn() {

        _ = _client.Execute(new RestRequest("/", Method.Get));

        var request = new RestRequest("public/login/", Method.Post);
        request.AddHeader("Content-Type", LOG_IN_FORM_CONTENT_TYPE);
        request.AddParameter("username", _username);
        request.AddParameter("password", _password);
        _ = _client.Execute(request);

    }

    public void Dispose() {
        _client.Dispose();
    }

}
