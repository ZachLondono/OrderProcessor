namespace ApplicationCore.Features.CADCode.Contracts.Machining;

internal record Route : Token {

    public Point StartPosition { get; init; } = new(0, 0);
    public Point EndPosition { get; init; } = new(0, 0);
    public float StartDepth { get; init; }
    public float EndDepth { get; init; }
    public RouteOffset Offset { get; init; } = new(OffsetType.None, 0);

}
