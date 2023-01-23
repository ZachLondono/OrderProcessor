using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

internal class CabinetDoorGaps {

    public Dimension TopGap { get; init; } = Dimension.Zero;
    public Dimension BottomGap { get; init; } = Dimension.Zero;
    public Dimension HorizontalGap { get; init; } = Dimension.Zero;
    public Dimension VerticalGap { get; init; } = Dimension.Zero;
    public Dimension EdgeReveal { get; init; } = Dimension.Zero;

}
