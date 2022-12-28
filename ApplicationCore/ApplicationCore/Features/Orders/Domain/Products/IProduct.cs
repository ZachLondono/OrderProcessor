namespace ApplicationCore.Features.Orders.Domain.Products;

public interface IProduct {
    public Guid Id { get; }
    public int Qty { get; }
    public decimal UnitPrice { get; }
}