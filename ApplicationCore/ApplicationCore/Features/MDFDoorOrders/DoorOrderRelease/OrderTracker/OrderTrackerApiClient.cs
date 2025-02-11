using RestSharp;

namespace ApplicationCore.Features.MDFDoorOrders.DoorOrderRelease.OrderTracker;

public class OrderTrackerApiClient {

    private RestClient _client;

    public OrderTrackerApiClient() {
#if DEBUG
        _client = new RestClient("http://localhost:5293/");
#else
        _client = new RestClient("http://api.zacharylondono.com");
#endif
    }

    public async Task<CreatedOrder?> PostNewOrder(NewOrder newOrder) {

        var request = new RestRequest("/orders");
        request.AddJsonBody(newOrder);

        var response = await _client.PostAsync<CreatedOrder>(request);

        return response;

    }

    public async Task<CreatedRelease?> PostNewMDFDoorRelease(Guid orderId, NewMDFDoorRelease release) {

        var request = new RestRequest($"/orders/{orderId}/mdf-door-releases");
        request.AddJsonBody(release);

        var response = await _client.PostAsync<CreatedRelease>(request);

        return response;

    }

}
