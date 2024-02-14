using RestSharp;

namespace ApplicationCore.Features.TrackOrder;

public class OrderTracker {

    public const string BASE_URL = "http://localhost:5293/";

    private readonly RestClient _client = new(BASE_URL);

    public async Task<OrderResponse?> PublishNewOrder(NewOrder order) {

        var request = new RestRequest("/orders");
        request.AddBody(order);

        var response = await _client.PostAsync<OrderResponse>(request);

        return response;

    }

    public async Task<OrderResponse?> AddMDFDoorReleaseToOrder(Guid orderId) {

        var release = new NewOrderRelease() {
            OrderId = orderId,
            Station = 1
        };

        var request = new RestRequest($"/orders/releases");
        request.AddBody(release);

        var response = await _client.PatchAsync<OrderResponse>(request);

        return response;

    }

}
