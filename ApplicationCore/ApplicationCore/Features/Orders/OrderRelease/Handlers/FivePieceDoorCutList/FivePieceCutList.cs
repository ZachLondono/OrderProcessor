namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.FivePieceDoorCutList;

public class FivePieceCutList {

    public string CustomerName { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string OrderName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public int TotalDoorCount { get; set; }
    public string Material { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public IEnumerable<LineItem> Items { get; set; } = Enumerable.Empty<LineItem>();

}
