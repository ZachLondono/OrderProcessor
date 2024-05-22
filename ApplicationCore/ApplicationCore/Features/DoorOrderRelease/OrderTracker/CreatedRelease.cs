namespace ApplicationCore.Features.DoorOrderRelease.OrderTracker;

public class CreatedRelease {

    public required Guid Id { get; init; }
    public required Guid OrderId { get; init; }
    public required int Type { get; init; }
    public required DateTime ReleaseDate { get; init; }
    public required DateTime? CompletedDate { get; init; }
    public required int Status { get; init; }
    public required int ItemCount { get; init; }

}