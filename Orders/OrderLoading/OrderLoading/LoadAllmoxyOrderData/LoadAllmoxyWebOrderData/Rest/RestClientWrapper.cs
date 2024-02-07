using RestSharp;

namespace OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyWebOrderData.Rest;

public class RestClientWrapper : IRestClient {

    private readonly RestClient _client;

    public RestClientWrapper(RestClient client) {
        _client = client;
    }

    public Task<RestResponse> ExecuteAsync(RestRequest request) => _client.ExecuteAsync(request);

    public void Dispose() => _client.Dispose();

}