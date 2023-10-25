namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.DovetailDBPackingList;

public class DovetailDrawerBoxPackingList {
    public DateTime OrderDate { get; set; }
    public string OrderName { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public Company Customer { get; set; } = new();
    public Company Vendor { get; set; } = new();
    public List<DovetailDrawerBox> Items { get; set; } = new();
}
