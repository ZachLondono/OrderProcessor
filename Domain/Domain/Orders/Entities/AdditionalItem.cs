namespace Domain.Orders.Entities;

/// <summary>
/// Represents a miscellaneous line item in an order
/// </summary>
public class AdditionalItem {

    public Guid Id { get; }
    public int Qty { get; }
    public string Description { get; } = string.Empty;
    public decimal UnitPrice { get; }

    public AdditionalItem(Guid id, int qty, string description, decimal price) {
        Id = id;
        Qty = qty;
        Description = description;
        UnitPrice = price;
    }

    public static AdditionalItem Create(int qty, string description, decimal price) => new(Guid.NewGuid(), qty, description, price);

}
