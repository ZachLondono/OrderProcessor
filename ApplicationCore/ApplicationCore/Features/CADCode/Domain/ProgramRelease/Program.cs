namespace ApplicationCore.Features.CADCode.Services.Domain.ProgramRelease;

public class ReleasedProgram {

    public string Name { get; init; } = string.Empty;
    public string ImagePath { get; init; } = string.Empty;
    public ProgramMaterial Material { get; init; } = new();
    public IReadOnlyList<NestedPart> Parts { get; init; } = new List<NestedPart>();

}
