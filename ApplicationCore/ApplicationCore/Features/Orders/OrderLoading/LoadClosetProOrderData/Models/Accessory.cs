namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.Models;

public class Accessory {
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ExportName { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public double Height { get; set; }
    public double Width { get; set; }
    public double Depth { get; set; }
    public int Quantity { get; set; }
    public string Cost { get; set; } = string.Empty;
}
