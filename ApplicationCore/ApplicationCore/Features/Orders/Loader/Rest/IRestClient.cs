using RestSharp;

namespace ApplicationCore.Features.Orders.Loader.Rest;

public interface IRestClient {

    public RestResponse Execute(RestRequest request);

    public void Dispose();

}
