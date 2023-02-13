namespace ApplicationCore.Features.Shared.Contracts;

public static class Ordering {

    public delegate Task<string> GetOrderNumberById(Guid orderId);

}