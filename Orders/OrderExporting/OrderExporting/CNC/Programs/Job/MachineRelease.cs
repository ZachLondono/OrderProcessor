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

    public IEnumerable<UsedEdgeBanding> GetUsedEdgeBanding() {

        return Programs.SelectMany(p => p.Parts)
                        .SelectMany(p => {

                            List<UsedEdgeBanding> eb = [];

                            if (p.Length1EdgeBanding is not null) {
                                eb.Add(new(p.Length1EdgeBanding, p.Length.AsMillimeters()));
                            }

                            if (p.Length2EdgeBanding is not null) {
                                eb.Add(new(p.Length2EdgeBanding, p.Length.AsMillimeters()));
                            }

                            if (p.Width1EdgeBanding is not null) {
                                eb.Add(new(p.Width1EdgeBanding, p.Length.AsMillimeters()));
                            }

                            if (p.Width2EdgeBanding is not null) {
                                eb.Add(new(p.Width2EdgeBanding, p.Length.AsMillimeters()));
                            }

                            return eb;

                        })
                        .GroupBy(e => e.Name)
                        .Select(g => new UsedEdgeBanding(g.Key, g.Sum(e => e.Length)));

    }

}
