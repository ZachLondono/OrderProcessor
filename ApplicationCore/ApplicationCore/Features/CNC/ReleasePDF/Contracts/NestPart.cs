using ApplicationCore.Features.CNC.Shared;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.CNC.ReleasePDF.Contracts;

public class NestedPart {

    public string Name { get; init; } = string.Empty;
    public string ImagePath { get; init; } = string.Empty;
    public Dimension Width { get; init; } = Dimension.FromMillimeters(0);
	public Dimension Length { get; init; } = Dimension.FromMillimeters(0);
    public string Description { get; init; } = string.Empty;
    public Point Center { get; init; } = new Point(0, 0);

}
