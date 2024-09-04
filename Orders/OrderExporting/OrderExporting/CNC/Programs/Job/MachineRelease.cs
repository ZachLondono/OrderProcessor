using OrderExporting.CNC.Programs.Domain;
using OrderExporting.CNC.Programs.WorkOrderReleaseEmail;

namespace OrderExporting.CNC.Programs.Job;

public class MachineRelease {

    public string MachineName { get; init; } = string.Empty;
    public IReadOnlyDictionary<int, string> ToolTable { get; init; } = new Dictionary<int, string>();
    public IEnumerable<ReleasedProgram> Programs { get; set; } = Enumerable.Empty<ReleasedProgram>();
    public IEnumerable<SinglePartProgram> SinglePrograms { get; set; } = Enumerable.Empty<SinglePartProgram>();
    public TableOrientation MachineTableOrientation { get; init; }

    public IEnumerable<UsedMaterial> GetUsedMaterials() {

        return Programs.Select(p => p.Material)
                       .GroupBy(m => (m.Name, m.Width, m.Length, m.Thickness, m.IsGrained))
                       .Select(g => new UsedMaterial(g.Count(), g.Key.Name, g.Key.Width, g.Key.Length, g.Key.Thickness));

    }

}
