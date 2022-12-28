namespace ApplicationCore.Features.CNC.GCode.Contracts.Options;

public class MachineGCodeOptions {
    public required string Name { get; set; }
    public required bool GenerateNestPrograms { get; set; }
    public required bool GenerateSinglePartPrograms { get; set; }
    public required bool GenerateLabels { get; set; }
}
