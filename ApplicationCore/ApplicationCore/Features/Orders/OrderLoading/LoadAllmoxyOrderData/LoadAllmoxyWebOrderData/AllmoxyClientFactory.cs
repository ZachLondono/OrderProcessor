using ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyWebOrderData.Rest;
using ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;
using Microsoft.Extensions.Options;
using RestSharp;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyWebOrderData;

public class AllmoxyClientFactory {

    private readonly AllmoxyCredentials _credentials;

    public AllmoxyClientFactory(IOptions<AllmoxyCredentials> credentials) {
        _credentials = credentials.Value;
    }

    public IAllmoxyClient CreateClient() {
        var client = new RestClientWrapper(new RestClient($"https://{_credentials.Instance}.allmoxy.com/"));
        return new AllmoxyClient(_credentials.Username, _credentials.Password, client);
    }

}