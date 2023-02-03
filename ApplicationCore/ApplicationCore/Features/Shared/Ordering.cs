namespace ApplicationCore.Features.Shared;

public static class Ordering {

    public delegate Task<string> GetOrderNumberById(Guid orderId);

}