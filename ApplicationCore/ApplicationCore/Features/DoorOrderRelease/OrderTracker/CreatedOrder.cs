namespace ApplicationCore.Features.DoorOrderRelease.OrderTracker;

public class CreatedOrder {

    public required Guid Id { get; init; }
    public required DateTime OrderedDate { get; init; }
    public required DateTime WantByDate { get; init; }
    public required DateTime? CompletedDate { get; init; }
    public required string Number { get; init; }
    public required string Name { get; init; }
    public required string Customer { get; init; }
    public required string Vendor { get; init; }
    public required string Note { get; init; }
    public required int Status { get; init; }
    public required bool IsRush { get; init; }
    public required int TotalItemCount { get; init; }
    public required decimal Price { get; init; }
    public required decimal Tax { get; init; }
    public required decimal Shipping { get; init; }

}
