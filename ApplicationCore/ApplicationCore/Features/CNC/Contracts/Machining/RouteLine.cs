namespace ApplicationCore.Features.CNC.Contracts.Machining;

public record RouteLine : Token {

    public Point StartPosition { get; init; } = new(0, 0);
    public Point EndPosition { get; init; } = new(0, 0);
    public double StartDepth { get; init; }
    public double EndDepth { get; init; }
    public RouteOffset Offset { get; init; } = new(OffsetType.None, 0);

}
