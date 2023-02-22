namespace ApplicationCore.Infrastructure.Bus;

internal record CachedResponse {

    public object Response { get; init; }
    public DateTime RecievedTimestamp { get; init; }

    public CachedResponse(object response, DateTime recievedTimestamp) {
        Response = response;
        RecievedTimestamp = recievedTimestamp;
    }

}
