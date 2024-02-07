using RestSharp;

namespace OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyWebOrderData.Rest;

public interface IRestClient {

    public Task<RestResponse> ExecuteAsync(RestRequest request);

    public void Dispose();

}
