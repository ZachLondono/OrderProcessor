using ApplicationCore.Features.CNC.Shared;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.CNC.GCode.Domain;

public record Tool {
    public required string Name { get; init; }
    public required Dimension Diameter { get; init; }
    public required Speed FeedSpeed { get; init; }
    public required Speed EntrySpeed { get; init; }
    public required Speed SpindleSpeed { get; init; }
    public required Speed CornerSpeed { get; init; }
}
