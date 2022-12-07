using ApplicationCore.Features.CNC.GCode.Domain;
using ApplicationCore.Features.CNC.GCode.Domain.CADCode;

namespace ApplicationCore.Features.CNC.GCode.Services;

public class MachineNestResult {
	public string MachineName { get; set; } = string.Empty;
	public string PictureFileOutputDirectory { get; set; } = string.Empty;
	public TableOrientation TableOrientation { get; set; }
	public IEnumerable<OptimizationResult> OptimizationResults { get; set; } = Enumerable.Empty<OptimizationResult>();
}
