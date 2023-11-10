namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.DoweledDrawerBoxCutList;

public class DoweledDrawerBoxCutList {

    public required string CustomerName { get; set; }
    public required string VendorName { get; set; }
    public required string OrderNumber { get; set; }
    public required string OrderName { get; set; }
    public required DateTime OrderDate { get; set; }
    public required int TotalBoxCount { get; set; }
    public required IEnumerable<DoweledDBCutListLineItem> Items { get; set; }
    public required string Material { get; set; }
    public required string Note { get; set; }

}
