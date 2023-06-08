using RestSharp;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyWebOrderData.Rest;

public interface IRestClient {

    public Task<RestResponse> ExecuteAsync(RestRequest request);

    public void Dispose();

}
