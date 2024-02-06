using Domain.ValueObjects;

namespace Domain.Orders.ValueObjects;

public readonly struct CabinetDoorGaps {

    public CabinetDoorGaps() { }

    /// <summary>
    /// Gap between the top of a cabinet door/drawer and the top of it's cabinet
    /// </summary>
    public required Dimension TopGap { get; init; } = Dimension.Zero;

    /// <summary>
    /// Gap between the bottom of a cabinet door and the bottom of it's cabinet
    /// </summary>
    public required Dimension BottomGap { get; init; } = Dimension.Zero;

    /// <summary>
    /// Gap between two horizontally adjacent doors/drawers
    /// </summary>
    public required Dimension HorizontalGap { get; init; } = Dimension.Zero;

    /// <summary>
    /// Gap between two vertically adjacent doors/drawers
    /// </summary>
    public required Dimension VerticalGap { get; init; } = Dimension.Zero;

    /// <summary>
    /// Gap between the left/right edge of a cabinet and the edge of the left/right most door
    /// </summary>
    public required Dimension EdgeReveal { get; init; } = Dimension.Zero;

}
