using Domain.Orders.ValueObjects;

namespace Domain.Orders.Entities.Products;

public interface IProduct {

    public Guid Id { get; }
    public int Qty { get; }
    public decimal UnitPrice { get; }
    public int ProductNumber { get; }
    public string Room { get; set; }

    public string GetDescription();
    public IEnumerable<Supply> GetSupplies();

    public List<string> ProductionNotes { get; }

}