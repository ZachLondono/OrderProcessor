namespace ApplicationCore.Features.TrackOrder;

public class OrderResponse {
    public required Guid Id { get; init; }
    public required DateTime OrderedDate { get; init; }
    public required string Number { get; init; } = string.Empty;
    public required string Name { get; init; } = string.Empty;
    public required string Customer { get; init; } = string.Empty;
    public required string Vendor { get; init; } = string.Empty;
    public required int Status { get; init; } = 0;
    public required DateTime? CompletedDate { get; init; } = null;
}