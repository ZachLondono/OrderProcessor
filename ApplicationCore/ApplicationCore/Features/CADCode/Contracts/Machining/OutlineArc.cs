namespace ApplicationCore.Features.CADCode.Contracts.Machining;

public record OutlineArc : OutlineSegment {

    public double Radius { get; init; } 
    public ArcDirection Direction { get; init; }

}