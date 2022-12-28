namespace ApplicationCore.Features.Orders.Domain.Products;

public interface IProduct
{

    public int Qty { get; }
    public decimal UnitPrice { get; }

}