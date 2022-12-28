namespace ApplicationCore.Features.CNC.GCode.Contracts.Machining;

public abstract record MachiningOperation {
    public string ToolName { get; init; } = string.Empty;
    public int Sequence { get; init; }
    public int PassCount { get; init; } = 1;
}
