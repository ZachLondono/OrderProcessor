namespace Domain.Orders.ValueObjects;

public record BillingInfo {

    public required string? InvoiceEmail { get; init; }
    public required string PhoneNumber { get; init; }
    public required Address Address { get; init; }

}