using RestSharp;

namespace ApplicationCore.Features.Orders.Loader.Rest;

public interface IRestClient {

    public Task<RestResponse> ExecuteAsync(RestRequest request);

    public void Dispose();

}
