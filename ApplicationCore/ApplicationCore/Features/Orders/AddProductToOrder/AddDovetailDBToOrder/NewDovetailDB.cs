namespace ApplicationCore.Features.Orders.AddProductToOrder.AddDovetailDBToOrder;

public class NewDovetailDB {

    public int Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public int ProductNumber { get; set; }
    public string Room { get; set; } = string.Empty;

    public double Height { get; set; }
    public double Width { get; set; }
    public double Depth { get; set; }

    public string BoxMaterial { get; set; } = string.Empty;
    public string BottomMaterial { get; set; } = string.Empty;

    public string Notches { get; set; } = string.Empty;
    public string Clips { get; set; } = string.Empty;
    public string Slides { get; set; } = string.Empty;
    public string Accessory { get; set; } = string.Empty;

    public string Note { get; set; } = string.Empty;

}