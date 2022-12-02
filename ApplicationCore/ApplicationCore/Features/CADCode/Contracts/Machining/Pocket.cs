namespace ApplicationCore.Features.CADCode.Contracts.Machining;

public record Pocket : Token {

    public Point PositionA { get; init; } = new(0, 0);
    public Point PositionB { get; init; } = new(0, 0);
    public Point PositionC { get; init; } = new(0, 0);
    public Point PositionD { get; init; } = new(0, 0);
    public double StartDepth { get; init; }
    public double EndDepth { get; init; }
    public int Climb { get; init; }

}
