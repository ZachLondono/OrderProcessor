namespace ApplicationCore.Features.CNC.GCode.Contracts.Options;

public class GCodeGenerationOptions {
    public IEnumerable<MachineGCodeOptions> Machines { get; set; } = Enumerable.Empty<MachineGCodeOptions>();
}
