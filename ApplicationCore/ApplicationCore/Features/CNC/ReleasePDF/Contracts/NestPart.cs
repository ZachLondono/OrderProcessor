using ApplicationCore.Features.CNC.Shared;

namespace ApplicationCore.Features.CNC.ReleasePDF.Contracts;

public class NestedPart {

    public string Name { get; init; } = string.Empty;
    public string ImagePath { get; init; } = string.Empty;
    public double Width { get; init; }
    public double Length { get; init; }
    public string Description { get; init; } = string.Empty;
    public Point Center { get; init; } = new Point(0, 0);

}
