namespace ApplicationCore.Features.DoorOrderRelease.OrderTracker;

public class NewOrder {
    public required string Number { get; set; }
    public required string Name { get; set; }
    public required string Customer { get; set; }
    public required string Vendor { get; set; }
    public required DateTime OrderedDate { get; set; }
    public required DateTime WantByDate { get; set; }
    public required bool IsRush { get; set; }
    public required decimal Price { get; set; }
    public required decimal Shipping { get; set; }
    public required decimal Tax { get; set; }
}
