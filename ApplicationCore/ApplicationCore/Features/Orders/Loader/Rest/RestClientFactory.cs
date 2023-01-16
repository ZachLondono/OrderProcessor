using RestSharp;

namespace ApplicationCore.Features.Orders.Loader.Rest;

public class RestClientFactory {

    public IRestClient CreateClient(string baseUrl) => new RestClientWrapper(new RestClient(baseUrl));

}
