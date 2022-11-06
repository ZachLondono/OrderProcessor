namespace ApplicationCore.Features.CADCode.Services.Domain.ProgramRelease;

public class MachineRelease {

    public string MachineName { get; init; } = string.Empty;
    public IReadOnlyDictionary<int, string> ToolTable { get; init; } = new Dictionary<int, string>();
    public IEnumerable<ReleasedProgram> Programs { get; init; } = new List<ReleasedProgram>();
    public TableOrientation MachineTableOrientation { get; init; }

}
