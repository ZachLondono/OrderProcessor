using ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;
using ApplicationCore.Features.Orders.Loader.Rest;

namespace ApplicationCore.Features.Orders.Loader;

public class AllmoxyClientFactory {

    private readonly AllmoxyCredentials _credentials;

    public AllmoxyClientFactory(AllmoxyCredentials credentials) {
        _credentials = credentials;
    }

    public IAllmoxyClient CreateClient() {
        var restFactory = new RestClientFactory();
        return new AllmoxyClient(_credentials.Instance, _credentials.Username, _credentials.Password, restFactory);
    }

}