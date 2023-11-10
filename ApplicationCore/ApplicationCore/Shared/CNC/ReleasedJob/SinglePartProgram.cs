using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Shared.CNC.ReleasedJob;

public class SinglePartProgram {

    public string Name { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public Dimension Width { get; init; } = Dimension.Zero;
    public Dimension Length { get; init; } = Dimension.Zero;
    public string Description { get; init; } = string.Empty;
    public string PartId { get; init; } = string.Empty;
    public string ProductNumber { get; init; } = string.Empty;
    public bool HasBackSideProgram { get; init; } = false;

}
