namespace ApplicationCore.Features.Shared;

public class Manufacturing {

    public delegate Task CreateWorkOrder(Guid orderId, string name, IReadOnlyCollection<Guid> productId);

    public delegate Task<bool> IsProductComplete(Guid orderId, Guid productId);

}
