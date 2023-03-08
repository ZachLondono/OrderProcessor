namespace ApplicationCore.Features.Orders.Contracts;

public static class Ordering {

    public delegate Task<string> GetOrderNumberById(Guid orderId);

}