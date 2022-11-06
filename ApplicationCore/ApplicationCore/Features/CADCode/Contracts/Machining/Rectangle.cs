namespace ApplicationCore.Features.CADCode.Contracts.Machining;

public record Rectangle : Token {

    public Point PositionA { get; init; } = new(0, 0);
    public Point PositionB { get; init; } = new(0, 0);
    public Point PositionC { get; init; } = new(0, 0);
    public Point PositionD { get; init; } = new(0, 0);
    public float StartDepth { get; init; }
    public float EndDepth { get; init; }
    public RouteOffset Offset { get; init; } = new(OffsetType.None, 0);

}
