namespace ApplicationCore.Features.CNC.GCode.Contracts.Options;

/// <summary>
/// Represents options that can be changed by a user when generating cnc programs
/// </summary>
public class GCodeGenerationOptions {
    public IEnumerable<MachineGCodeOptions> Machines { get; set; } = Enumerable.Empty<MachineGCodeOptions>();
}
