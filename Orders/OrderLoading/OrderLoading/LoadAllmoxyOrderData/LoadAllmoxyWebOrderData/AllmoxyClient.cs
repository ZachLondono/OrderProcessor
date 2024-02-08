using RestSharp;
using IRestClient = OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyWebOrderData.Rest.IRestClient;

namespace OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyWebOrderData;

public class AllmoxyClient : IAllmoxyClient {

    private readonly IRestClient _client;
    private readonly string _username;
    private readonly string _password;

    public const string NOT_LOGGED_IN_CONTENT_TYPE = "text/html";
    public const string EXPORT_CONTENT_TYPE = "application/xml";
    public const string LOG_IN_FORM_CONTENT_TYPE = "application/x-www-form-urlencoded";
    public const string EXPORT_NOT_FOUND_CONTENT = "Find: No matching Exporter";
    public const string ORDER_NOT_FOUND_CONTENT = "Find: No matching Order"; //(####) records found";
    private const int MAX_RETRIES = 3;


    public AllmoxyClient(string username, string password, IRestClient client) {
        _username = username;
        _password = password;

        _client = client;
    }

    public Task<string> GetExportAsync(string orderNumber, int index) => GetExportAsync(orderNumber, index, 0);

    private async Task<string> GetExportAsync(string orderNumber, int index, int tries) {

        if (tries > MAX_RETRIES) {
            throw new InvalidOperationException("Could not log in to Allmoxy");
        }

        var request = new RestRequest($"orders/export_partlist/{orderNumber}/{index}/", Method.Get);
        RestResponse response = await _client.ExecuteAsync(request);

        switch (response.ContentType) {
            case NOT_LOGGED_IN_CONTENT_TYPE:

                if (response.Content is not null) {
                    if (response.Content.Contains(EXPORT_NOT_FOUND_CONTENT)) {
                        throw new InvalidOperationException("Export could not be found");
                    } else if (response.Content.Contains(ORDER_NOT_FOUND_CONTENT)) {
                        throw new InvalidOperationException("Order could not be found");
                    }
                }

                await LogIn();
                return await GetExportAsync(orderNumber, index, ++tries);

            case EXPORT_CONTENT_TYPE:
                if (response.Content is null) {
                    throw new InvalidOperationException("No data returned");
                }
                return response.Content;

            default:
                throw new InvalidOperationException($"Unexpected response from server {response.StatusCode}");
        }

    }

    private async Task LogIn() {

        _ = await _client.ExecuteAsync(new RestRequest("/", Method.Get));

        var request = new RestRequest("public/login/", Method.Post);
        request.AddHeader("Content-Type", LOG_IN_FORM_CONTENT_TYPE);
        request.AddParameter("username", _username);
        request.AddParameter("password", _password);
        _ = await _client.ExecuteAsync(request);

    }

    public void Dispose() {
        _client.Dispose();
    }

}
