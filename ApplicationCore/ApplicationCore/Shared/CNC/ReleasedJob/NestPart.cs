using ApplicationCore.Shared.CNC.Domain;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Shared.CNC.ReleasedJob;

public class NestedPart {

    public string Name { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public bool HasFace6 { get; init; } = false;
    public string? Face6FileName { get; set; } = null;
    public string ImageData { get; init; } = string.Empty;
    public Dimension Width { get; init; } = Dimension.FromMillimeters(0);
    public Dimension Length { get; init; } = Dimension.FromMillimeters(0);
    public string Description { get; init; } = string.Empty;
    public Point Center { get; init; } = new Point(0, 0);
    public string ProductNumber { get; init; } = string.Empty;
    public Guid ProductId { get; init; } = Guid.Empty;
    public string PartId { get; init; } = string.Empty;
    public bool IsRotated { get; init; } = false;
    public bool HasBackSideProgram { get; init; } = false;
    public string Note { get; init; } = string.Empty;

}
