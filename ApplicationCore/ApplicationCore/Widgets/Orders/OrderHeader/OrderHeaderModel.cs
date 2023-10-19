namespace ApplicationCore.Widgets.Orders.OrderHeader;

public class OrderHeaderModel {
    public Guid OrderId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CustomerComment { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public Guid VendorId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? DueDate { get; set; }
    public bool Rush { get; set; }
}
