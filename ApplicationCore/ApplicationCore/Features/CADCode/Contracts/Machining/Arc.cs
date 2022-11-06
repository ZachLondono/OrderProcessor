namespace ApplicationCore.Features.CADCode.Contracts.Machining;

internal record Arc : Token {

    public Point PositionA { get; init; } = new(0, 0);
    public Point PositionB { get; init; } = new(0, 0);
    public Point PositionC { get; init; } = new(0, 0);
    public Point PositionD { get; init; } = new(0, 0);
    public float StartDepth { get; init; }
    public float EndDepth { get; init; }
    public float Radius { get; init; }
    public ArcDirection Direction { get; init; } = ArcDirection.Unknown;
    public RouteOffset Offset { get; init; } = new(OffsetType.None, 0);
    public float Bulge { get; init; }

}
