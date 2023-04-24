using ApplicationCore.Features.CNC.Domain;

namespace ApplicationCore.Features.CNC.Contracts;

public class MachineRelease {

    public string MachineName { get; init; } = string.Empty;
    public IReadOnlyDictionary<int, string> ToolTable { get; init; } = new Dictionary<int, string>();
    public IEnumerable<ReleasedProgram> Programs { get; set; } = new List<ReleasedProgram>();
    public TableOrientation MachineTableOrientation { get; init; }

}
