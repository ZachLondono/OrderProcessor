using RestSharp;

namespace ApplicationCore.Features.Orders.Loader.Rest;

public class RestClientWrapper : IRestClient {

    private readonly RestClient _client;

    public RestClientWrapper(RestClient client) {
        _client = client;
    }

    public RestResponse Execute(RestRequest request) => _client.Execute(request);

    public void Dispose() => _client.Dispose();

}