namespace ApplicationCore.Features.CNC.Contracts.Machining;

public record OutlineArc : OutlineSegment {

    public double Radius { get; init; } 
    public ArcDirection Direction { get; init; }

}