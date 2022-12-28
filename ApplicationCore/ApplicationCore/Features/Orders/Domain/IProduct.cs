namespace ApplicationCore.Features.Orders.Domain;

public interface IProduct {

	public int Qty { get; }
	public decimal UnitPrice { get; }

}