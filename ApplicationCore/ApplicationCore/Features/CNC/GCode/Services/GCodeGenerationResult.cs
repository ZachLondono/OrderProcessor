namespace ApplicationCore.Features.CNC.GCode.Services;

public class GCodeGenerationResult {
	public string BatchName { get; set; } = string.Empty;
	public IEnumerable<MachineNestResult> MachineResults { get; set; } = Enumerable.Empty<MachineNestResult>();
}
