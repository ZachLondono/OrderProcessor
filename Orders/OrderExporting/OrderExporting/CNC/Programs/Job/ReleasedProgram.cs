namespace OrderExporting.CNC.Programs.Job;

public class ReleasedProgram {

    public string Name { get; init; } = string.Empty;
    public string ImagePath { get; init; } = string.Empty;
    public bool HasFace6 { get; set; } = false;
    public ProgramMaterial Material { get; init; } = new();
    public IReadOnlyList<NestedPart> Parts { get; init; } = new List<NestedPart>();

}
