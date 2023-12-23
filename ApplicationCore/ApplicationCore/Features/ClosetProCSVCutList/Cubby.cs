using ApplicationCore.Features.ClosetProCSVCutList.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

namespace ApplicationCore.Features.ClosetProCSVCutList;

public class Cubby {

    public required DividerShelf TopDividerShelf { get; init; }
    public required DividerShelf BottomDividerShelf { get; init; }
    public required DividerVerticalPanel[] DividerPanels { get; init; }
    public required Shelf[] FixedShelves { get; init; }
    public required ClosetMaterial Material { get; init; }
    public required string EdgeBandingColor { get; init; }
    public required string Room { get; init; }

}
