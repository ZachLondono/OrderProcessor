namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record ShippingInfo {

    public required string Method { get; init; }
    public required Address Address { get; init; }
    public required decimal Price { get; init; }
    public required string Contact { get; init; }
    public required string PhoneNumber { get; init; }

}
