namespace ApplicationCore.Features.CNC.Contracts.Machining;

public record PocketArc : PocketSegment {

    public double Radius { get; init; }
    public ArcDirection Direction { get; init; }

}