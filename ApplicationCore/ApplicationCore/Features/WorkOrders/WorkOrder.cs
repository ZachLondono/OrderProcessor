namespace ApplicationCore.Features.WorkOrders;

public class WorkOrder {

    public Guid Id { get; }
    public string Name { get; set; }
    public Guid OrderId { get; }
    public IReadOnlyCollection<Guid> ProductIds { get; }
    public Status Status { get; set; }

    public WorkOrder(Guid id, string name, Guid orderId, IReadOnlyCollection<Guid> productIds, Status status) {
        Id = id;
        Name = name;
        OrderId = orderId;
        ProductIds = productIds;
        Status = status;
    }

}
