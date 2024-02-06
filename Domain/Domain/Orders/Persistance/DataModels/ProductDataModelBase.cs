namespace Domain.Orders.Persistance.DataModels;

public abstract class ProductDataModelBase {

    public Guid Id { get; set; }
    public int Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public int ProductNumber { get; set; }
    public string Room { get; set; } = string.Empty;

}
