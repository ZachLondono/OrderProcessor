using Domain.ValueObjects;

namespace ApplicationCore.Shared.CNC.Job;

public class SinglePartProgram {

    public string Name { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public Dimension Width { get; init; } = Dimension.Zero;
    public Dimension Length { get; init; } = Dimension.Zero;
    public string Description { get; init; } = string.Empty;
    public string PartId { get; init; } = string.Empty;
    public string ProductNumber { get; init; } = string.Empty;

    /// <summary>
    /// Indicates that this part has a separate single program for the other face (NOT a face 6 program)
    /// </summary>
    public bool HasBackSideProgram { get; init; } = false;

}
