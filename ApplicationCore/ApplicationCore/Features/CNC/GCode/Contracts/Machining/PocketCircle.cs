using ApplicationCore.Features.CNC.Shared;

namespace ApplicationCore.Features.CNC.GCode.Contracts.Machining;

public record PocketCircle : CompositeToken {

    // TODO: all pocket circle tokens must be added last to the CADCode code class
    public Point CenterPosition { get; init; } = new();
    public double Radius { get; init; }
    public double Depth { get; init; }

    public override IEnumerable<MachiningOperation> GetComponents() {

        return new List<MachiningOperation>() {
            new PocketArc() {
                StartPosition = new(CenterPosition.X - Radius, CenterPosition.Y),
                StartDepth = Depth,
                EndPosition = new(CenterPosition.X + Radius, CenterPosition.Y),
                EndDepth = Depth,
                Radius = Radius,
                Direction = ArcDirection.Clockwise,
                Offset = new(OffsetType.None, 0),
                PassCount = PassCount,
                ToolName = ToolName
            },
            new PocketArc() {
                StartPosition = new(CenterPosition.X + Radius, CenterPosition.Y),
                StartDepth = Depth,
                EndPosition = new(CenterPosition.X - Radius, CenterPosition.Y),
                EndDepth = Depth,
                Radius = Radius,
                Direction = ArcDirection.Clockwise,
                Offset = new(OffsetType.None, 0),
                PassCount = PassCount,
                ToolName = ToolName
            }
        };

    }
}