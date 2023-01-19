using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Loader.Providers.DTO;

public record OrderData {

    public required string Number { get; init; } = string.Empty;

    public required string Name { get; init; } = string.Empty;

    public required string Comment { get; init; } = string.Empty;

    public required decimal Tax { get; init; }

    public required decimal PriceAdjustment { get; init; }

    public required ShippingInfo Shipping { get; init; }

    public required BillingInfo Billing { get; init; }

    public required DateTime OrderDate { get; init; }

    public required Customer Customer { get; init; }

    public required Guid VendorId { get; init; }

    public required bool Rush { get; init; }

    public required Dictionary<string, string> Info { get; init; } = new();

    public required List<IProduct> Products { get; init; } = new();

    public required List<AdditionalItem> AdditionalItems { get; init; } = new();

}
