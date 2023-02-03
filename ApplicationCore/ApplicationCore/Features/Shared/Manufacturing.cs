namespace ApplicationCore.Features.Shared;

public static class Manufacturing {

    public delegate Task<Guid> CreateWorkOrder(Guid orderId, string name, IReadOnlyCollection<Guid> productId);

    public delegate Task<bool> IsProductComplete(Guid orderId, Guid productId);

}
