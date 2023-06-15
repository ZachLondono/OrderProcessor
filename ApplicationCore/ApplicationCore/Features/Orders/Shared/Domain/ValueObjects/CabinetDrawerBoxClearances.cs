using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

internal class CabinetDrawerBoxClearances {

    /// <summary>
    /// Clearance on each side of a drawer box
    /// </summary>
    public Dimension WidthClearance { get; init; } = Dimension.Zero;

    /// <summary>
    /// Total vertical clearance for drawerbox
    /// </summary>
    public Dimension HeightClearance { get; init; } = Dimension.Zero;

    /// <summary>
    /// Available standard drawer box heights
    /// </summary>
    public Dimension[] AvailableHeights { get; init; } = Array.Empty<Dimension>();

}