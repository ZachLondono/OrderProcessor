namespace ApplicationCore.Features.Orders.ProductDrawings.Models;

public class ProductDrawingSummary {

    public required Guid Id { get; set; }
    public required Guid ProductId { get; set; }
    public required string Name { get; set; }

};
