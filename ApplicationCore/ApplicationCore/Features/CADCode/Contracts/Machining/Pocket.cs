namespace ApplicationCore.Features.CADCode.Contracts.Machining;

internal record Pocket : Token {

    public Point PositionA { get; init; } = new(0, 0);
    public Point PositionB { get; init; } = new(0, 0);
    public Point PositionC { get; init; } = new(0, 0);
    public Point PositionD { get; init; } = new(0, 0);
    public float StartDepth { get; init; }
    public float EndDepth { get; init; }
    public int Climb { get; init; }

}
