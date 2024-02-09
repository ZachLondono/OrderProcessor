using OrderExporting.CNC.Programs.Domain;

namespace OrderExporting.CNC.Programs.Job;

public class MachineRelease {

    public string MachineName { get; init; } = string.Empty;
    public IReadOnlyDictionary<int, string> ToolTable { get; init; } = new Dictionary<int, string>();
    public IEnumerable<ReleasedProgram> Programs { get; set; } = Enumerable.Empty<ReleasedProgram>();
    public IEnumerable<SinglePartProgram> SinglePrograms { get; set; } = Enumerable.Empty<SinglePartProgram>();
    public TableOrientation MachineTableOrientation { get; init; }

}
