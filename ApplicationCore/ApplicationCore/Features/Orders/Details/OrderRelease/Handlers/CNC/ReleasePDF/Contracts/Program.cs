namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Contracts;

public class ReleasedProgram {

    public string Name { get; init; } = string.Empty;
    public string ImagePath { get; init; } = string.Empty;
    public bool HasFace6 { get; init; } = false;
    public ProgramMaterial Material { get; init; } = new();
    public IReadOnlyList<NestedPart> Parts { get; init; } = new List<NestedPart>();

}
