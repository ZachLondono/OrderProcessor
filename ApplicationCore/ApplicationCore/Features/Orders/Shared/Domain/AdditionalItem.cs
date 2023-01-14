namespace ApplicationCore.Features.Orders.Shared.Domain;

public class AdditionalItem {

    public Guid Id { get; }
    public string Description { get; } = string.Empty;
    public decimal Price { get; }

    public AdditionalItem(Guid id, string description, decimal price) {
        Id = id;
        Description = description;
        Price = price;
    }

    public static AdditionalItem Create(string description, decimal price) => new(Guid.NewGuid(), description, price);

}
