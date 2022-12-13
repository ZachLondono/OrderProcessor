namespace ApplicationCore.Features.CNC.GCode.Contracts.Results;

public record MachineGenerationResult {
    public required string MachineName { get; set; }
    public required OptimizationResult? OptimizationResults { get; set; }
    public required SinglePartGenerationResult? SinglePartResults { get; set; }
}