using ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

namespace ApplicationCore.Features.Orders.Loader;

public class AllmoxyLoaderFactory {

    private readonly AllmoxyCredentials _credentials;

    public AllmoxyLoaderFactory(AllmoxyCredentials credentials) {
        _credentials = credentials;
    }

    public IAllmoxyClient CreateClient() {
        return new AllmoxyClient(_credentials.Instance, _credentials.Username, _credentials.Password);
    }

}