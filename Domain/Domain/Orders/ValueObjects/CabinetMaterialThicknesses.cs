using Domain.ValueObjects;

namespace Domain.Orders.ValueObjects;

public readonly struct CabinetConstruction {

    public CabinetConstruction() { }

    public required Dimension TopThickness { get; init; }

    public required Dimension BottomThickness { get; init; }

    public required Dimension SideThickness { get; init; }

    public required Dimension BackThickness { get; init; }

    /// <summary>
    /// Distance that the back panel is inset into the cabinet. Distance from edge of bottom/sides to the start of the back Dado.
    /// </summary>
    public required Dimension BackInset { get; init; }

}