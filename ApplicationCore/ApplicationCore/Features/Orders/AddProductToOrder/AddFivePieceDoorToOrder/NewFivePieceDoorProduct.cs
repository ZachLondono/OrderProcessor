namespace ApplicationCore.Features.Orders.AddProductToOrder.AddFivePieceDoorToOrder;

public class NewFivePieceDoorProduct {

    public int Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public int ProductNumber { get; set; }
    public string Room { get; set; } = string.Empty;

    public double Width { get; set; }
    public double Height { get; set; }
    public double Stiles { get; set; }
    public double Rails { get; set; }

    public string Material { get; set; } = string.Empty;

}
