using RestSharp;

namespace ApplicationCore.Features.Orders.Loader;

internal class AllmoxyClient : IAllmoxyClient {

    private readonly RestClient _client;
    private readonly string _instanceName;
    private readonly string _username;
    private readonly string _password;

    private string BaseUrl => $"https://{_instanceName}.allmoxy.com/";

    public AllmoxyClient(string instanceName, string username, string password) {
        _instanceName = instanceName;
        _username = username;
        _password = password;

        _client = new RestClient(new RestClientOptions(BaseUrl) {
            FollowRedirects = true // Following redirects is required to successfully log in
        });

    }

    public string GetExport(string orderNumber, int index) {

        var request = new RestRequest($"orders/export_partlist/{orderNumber}/{index}/", Method.Get);
        var response = _client.Execute(request);

        switch (response.ContentType) {
            case "text/html":
                LogIn();
                return GetExport(orderNumber, index);

            case "application/xml":
                return response.Content ?? "empty";

            default:
                throw new InvalidOperationException($"Unexpected response from server {response.StatusCode}");
        }

    }

    private void LogIn() {

        _ = _client.Execute(new RestRequest("/", Method.Get));

        var request = new RestRequest("public/login/", Method.Post);
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request.AddParameter("username", _username);
        request.AddParameter("password", _password);
        _ = _client.Execute(request);

    }

    public void Dispose() {
        _client.Dispose();
        //GC.SuppressFinalize(this);
    }

}
