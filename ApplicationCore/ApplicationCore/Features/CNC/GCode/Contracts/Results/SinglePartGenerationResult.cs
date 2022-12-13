namespace ApplicationCore.Features.CNC.GCode.Contracts.Results;

public record SinglePartGenerationResult {
    public IEnumerable<string> GeneratedFiles { get; init; } = Enumerable.Empty<string>();
}
