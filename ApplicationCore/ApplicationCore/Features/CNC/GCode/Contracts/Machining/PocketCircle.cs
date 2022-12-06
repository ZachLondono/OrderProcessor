using ApplicationCore.Features.CNC.Shared;

namespace ApplicationCore.Features.CNC.GCode.Contracts.Machining;

public record PocketCircle : CompositeToken
{

	// TODO: all pocket circle tokens must be added last to 

    public Point CenterPosition { get; init; } = new();
    public double Radius { get; init; }
	public double Depth { get; init; }

	public override IEnumerable<Token> GetComponents()
	{

		return new List<Token>() {
			new PocketArc()
			{
				StartPosition = new(CenterPosition.X - Radius, CenterPosition.Y),
				StartDepth = Depth,
				EndPosition = new(CenterPosition.X + Radius, CenterPosition.Y),
				EndDepth = Depth,
				Radius = Radius,
				Direction = ArcDirection.Clockwise,
				Offset = new(OffsetType.None, 0),
				PassCount = PassCount,
				RType = RType,
				Sequence = Sequence,
				Tool = Tool
			},
			new PocketArc()
			{
				StartPosition = new(CenterPosition.X + Radius, CenterPosition.Y),
				StartDepth = Depth,
				EndPosition = new(CenterPosition.X - Radius, CenterPosition.Y),
				EndDepth = Depth,
				Radius = Radius,
				Direction = ArcDirection.Clockwise,
				Offset = new(OffsetType.None, 0),
				PassCount = PassCount,
				RType = RType,
				Sequence = Sequence,
				Tool = Tool
			}
		};

	}
}