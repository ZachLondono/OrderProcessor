using ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;
using ApplicationCore.Features.Orders.Loader.Rest;
using RestSharp;

namespace ApplicationCore.Features.Orders.Loader;

public class AllmoxyClientFactory {

    private readonly AllmoxyCredentials _credentials;

    public AllmoxyClientFactory(AllmoxyCredentials credentials) {
        _credentials = credentials;
    }

    public IAllmoxyClient CreateClient() {
        var client = new RestClientWrapper(new RestClient($"https://{_credentials.Instance}.allmoxy.com/"));
        return new AllmoxyClient(_credentials.Username, _credentials.Password, client);
    }

}