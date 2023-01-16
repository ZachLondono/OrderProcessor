using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

internal class MDFDoorConfiguration {

    public required string Material { get; init; }
    public required string FramingBead { get; init; }
    public required string EdgeDetail { get; init; }
    public required Dimension TopRail { get; init; }
    public required Dimension BottomRail { get; init; }
    public required Dimension LeftStile { get; init; }
    public required Dimension RightStile { get; init; }

}