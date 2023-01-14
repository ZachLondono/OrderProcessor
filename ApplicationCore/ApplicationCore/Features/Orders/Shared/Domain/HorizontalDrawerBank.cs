using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain;

public record HorizontalDrawerBank {

    public required Dimension FaceHeight { get; init; }
    public required int Quantity { get; init; }
    public required CabinetDrawerBoxMaterial BoxMaterial { get; init; }
    public required DrawerSlideType SlideType { get; init; }

    public Dimension GetBoxWidth(Dimension innerCabWidth, Dimension dividerThickness, Func<DrawerSlideType, Dimension> getSlideWidthAdjustment) {

        // Between each drawer box there are 2 dividers
        int dividerCount = (Quantity - 1) * 2;

        Dimension availableWidth = innerCabWidth - (dividerCount * dividerThickness);

        return (availableWidth / Quantity) - getSlideWidthAdjustment(SlideType);

    }

    public Dimension GetBoxHeight(Dimension verticalClearance, IEnumerable<Dimension> availableHeights) {

        // TODO: calculate box height
        return availableHeights.OrderDescending().First();

    }

}