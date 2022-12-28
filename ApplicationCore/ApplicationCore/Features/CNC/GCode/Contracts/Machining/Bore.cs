using ApplicationCore.Features.CNC.Shared;

namespace ApplicationCore.Features.CNC.GCode.Contracts.Machining;

public record Bore : MachiningOperation {
    public Point Position { get; init; } = new(0, 0);
    public double Depth { get; init; }
}
