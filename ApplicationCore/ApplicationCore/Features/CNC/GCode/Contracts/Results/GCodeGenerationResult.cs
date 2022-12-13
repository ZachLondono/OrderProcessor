namespace ApplicationCore.Features.CNC.GCode.Contracts.Results;

public record GCodeGenerationResult {
    public string BatchName { get; init; } = string.Empty;
    public required IEnumerable<MachineGenerationResult> MachineResults { get; init; }
}
