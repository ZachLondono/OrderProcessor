using ApplicationCore.Features.CNC.Domain;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.CNC.Contracts;

public class NestedPart {

    public string Name { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public bool HasFace6 { get; init; } = false;
    public string ImageData { get; init; } = string.Empty;
    public Dimension Width { get; init; } = Dimension.FromMillimeters(0);
    public Dimension Length { get; init; } = Dimension.FromMillimeters(0);
    public string Description { get; init; } = string.Empty;
    public Point Center { get; init; } = new Point(0, 0);
    public string ProductNumber { get; init; } = string.Empty;
    public Guid ProductId { get; init; } = Guid.Empty;
    public string PartId { get; init; } = string.Empty;
    public bool IsRotated { get; init; } = false;

}
