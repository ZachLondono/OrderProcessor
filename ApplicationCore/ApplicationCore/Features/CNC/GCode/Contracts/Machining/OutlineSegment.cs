namespace ApplicationCore.Features.CNC.GCode.Contracts.Machining;

public record OutlineSegment : Token
{

    public Point Start { get; init; } = new(0, 0);
    public Point End { get; init; } = new(0, 0);
    public RouteOffset Offset { get; init; } = new(OffsetType.None, 0);

}
