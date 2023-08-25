namespace ApplicationCore.Features.Orders.Shared.Domain.Entities;

/// <summary>
/// Represents a miscellaneous line item in an order
/// </summary>
public class AdditionalItem {

    public Guid Id { get; }
    public string Description { get; } = string.Empty;
    public decimal Price { get; }
    public bool IsService { get; }

    public AdditionalItem(Guid id, string description, decimal price, bool isService = false) {
        Id = id;
        Description = description;
        Price = price;
        IsService = isService;
    }

    public static AdditionalItem Create(string description, decimal price, bool isService = false) => new(Guid.NewGuid(), description, price, isService);

}
