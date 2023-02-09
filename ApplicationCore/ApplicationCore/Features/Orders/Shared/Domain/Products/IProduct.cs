namespace ApplicationCore.Features.Orders.Shared.Domain.Products;

public interface IProduct {
    public Guid Id { get; }
    public int Qty { get; }
    public decimal UnitPrice { get; }
    public string Description { get; }
    public int ProductNumber { get; }
}