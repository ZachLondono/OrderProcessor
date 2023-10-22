namespace ApplicationCore.Features.Orders.ProductDrawings.Models;

public class ProductDrawing {

    public required Guid Id { get; set; }
    public required Guid ProductId { get; set; }
    public required byte[] DXFData { get; set; }
    public required string Name { get; set; }

};
