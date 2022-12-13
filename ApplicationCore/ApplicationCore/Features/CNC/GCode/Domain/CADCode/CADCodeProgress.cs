namespace ApplicationCore.Features.CNC.GCode.Domain.CADCode;

public record CADCodeProgress {
	public string Message { get; init; } = string.Empty;
	public int Progress { get; init; }
}
