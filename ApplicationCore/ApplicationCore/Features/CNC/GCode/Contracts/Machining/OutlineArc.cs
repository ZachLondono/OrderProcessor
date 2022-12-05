using ApplicationCore.Features.CNC.Shared;

namespace ApplicationCore.Features.CNC.GCode.Contracts.Machining;

public record OutlineArc : OutlineSegment
{

    public Point Center { get; init; } = new(0, 0);
    public double Radius { get; init; }
    public ArcDirection Direction { get; init; }

}