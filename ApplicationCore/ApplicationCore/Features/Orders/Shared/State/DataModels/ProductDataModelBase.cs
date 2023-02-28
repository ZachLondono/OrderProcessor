namespace ApplicationCore.Features.Orders.Shared.State.DataModels;

internal abstract class ProductDataModelBase {

    public Guid Id { get; set; }
    public int Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public int ProductNumber { get; set; }
}
