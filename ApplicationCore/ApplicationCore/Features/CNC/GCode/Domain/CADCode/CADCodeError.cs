namespace ApplicationCore.Features.CNC.GCode.Domain.CADCode;

public record CADCodeError {
	public string Message { get; init; } = string.Empty;
}
