namespace ApplicationCore.Features.Orders.Shared.Domain;

public record Customer {

    public required string Name { get; init;  }
    public string? InvoiceEmail { get; init; } = null;

}