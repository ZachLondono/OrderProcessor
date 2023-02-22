using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.Domain;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Contracts;

public class MachineRelease {

    public string MachineName { get; init; } = string.Empty;
    public IReadOnlyDictionary<int, string> ToolTable { get; init; } = new Dictionary<int, string>();
    public IEnumerable<ReleasedProgram> Programs { get; init; } = new List<ReleasedProgram>();
    public TableOrientation MachineTableOrientation { get; init; }

}
