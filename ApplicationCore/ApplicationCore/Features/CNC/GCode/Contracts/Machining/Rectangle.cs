using ApplicationCore.Features.CNC.GCode.Domain;
using System.Diagnostics;
using System.Linq;

namespace ApplicationCore.Features.CNC.GCode.Contracts.Machining;

public record Rectangle : CompositeToken
{

    public Point PositionA { get; init; } = new(0, 0);
    public Point PositionB { get; init; } = new(0, 0);
    public Point PositionC { get; init; } = new(0, 0);
    public Point PositionD { get; init; } = new(0, 0);
    public double Radius { get; init; }
    public double StartDepth { get; init; }
    public double EndDepth { get; init; }
    public RouteOffset Offset { get; init; } = new(OffsetType.None, 0);

    public override IEnumerable<Token> GetComponents()
    {

        Shape shape = new();

        var midPoint = new Point((PositionA.X + PositionB.X) / 2, (PositionA.Y + PositionB.Y) / 2);

        shape.AddLine(midPoint, PositionB);
        shape.AddFillet(Radius);
        shape.AddLine(PositionB, PositionC);
        shape.AddFillet(Radius);
        shape.AddLine(PositionC, PositionD);
        shape.AddFillet(Radius);
        shape.AddLine(PositionD, PositionA);
        shape.AddFillet(Radius);
        shape.AddLine(PositionA, midPoint);

        var segments = shape.GetSegments();

        return segments.Select<ShapeSegment, Token>(s =>
        {
            if (s is LineSegment line)
                return new RouteLine()
                {
                    StartPosition = line.Start,
                    EndPosition = line.End,
                    Offset = Offset,
                    StartDepth = StartDepth,
                    EndDepth = EndDepth,
                    PassCount = PassCount,
                    RType = RType,
                    Sequence = Sequence,
                    Tool = Tool,
                };
            else if (s is ArcSegment arc)
                return new RouteArc()
                {
                    StartPosition = arc.Start,
                    EndPosition = arc.End,
                    Radius = arc.Radius,
                    Direction = arc.Direction,
                    Offset = Offset,
                    StartDepth = StartDepth,
                    EndDepth = EndDepth,
                    PassCount = PassCount,
                    RType = RType,
                    Sequence = Sequence,
                    Tool = Tool,
                };

            throw new UnreachableException();
        }).Cast<Token>();

    }
}
