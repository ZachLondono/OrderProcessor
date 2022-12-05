using ApplicationCore.Features.CNC.Shared;

namespace ApplicationCore.Features.CNC.GCode.Contracts.Machining;

public record RouteArc : Token
{

    public Point StartPosition { get; init; } = new(0, 0);
    public Point EndPosition { get; init; } = new(0, 0);
    public double StartDepth { get; init; }
    public double EndDepth { get; init; }
    public Point Center { get; init; } = new(0, 0);
    public RouteOffset Offset { get; init; } = new(OffsetType.None, 0);
    public double Radius { get; init; }
    public ArcDirection Direction { get; init; } = ArcDirection.Unknown;

}
