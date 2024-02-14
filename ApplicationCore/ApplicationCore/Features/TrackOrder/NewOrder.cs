namespace ApplicationCore.Features.TrackOrder;

public class NewOrder {
    public required string Number { get; set; }
    public required string Name { get; set; }
    public required string Customer { get; set; }
    public required string Vendor { get; set; }
    public required DateTime OrderedDate { get; set; }
}

