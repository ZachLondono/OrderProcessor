namespace ApplicationCore.Features.Orders.Domain;

public class AdditionalItem {

    public Guid Id { get; }
    public string Description { get; } = string.Empty;
    public decimal Price { get; }

    public AdditionalItem(Guid id, string description, decimal price) {
        Id = id;
        Description = description;
        Price = price;
    }

}
