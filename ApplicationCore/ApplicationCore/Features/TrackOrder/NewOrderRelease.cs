namespace ApplicationCore.Features.TrackOrder;

public class NewOrderRelease {
    public required Guid OrderId { get; set; }
    public required int Station { get; set; }
}