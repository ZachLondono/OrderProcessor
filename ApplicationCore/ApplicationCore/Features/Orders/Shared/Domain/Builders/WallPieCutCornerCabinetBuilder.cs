using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

internal class WallPieCutCornerCabinetBuilder : CabinetBuilder<WallPieCutCornerCabinet> {

    public Dimension RightWidth { get; private set; }
    public Dimension RightDepth { get; private set; }
    public int AdjustableShelves { get; private set; }
    public HingeSide HingeSide { get; private set; }
    public Dimension ExtendDown { get; private set; }

    public WallPieCutCornerCabinetBuilder() {
        RightWidth = Dimension.Zero;
        RightDepth = Dimension.Zero;
        AdjustableShelves = 0;
        HingeSide = HingeSide.Left;
    }

    public WallPieCutCornerCabinetBuilder WithRightWidth(Dimension rightWidth) {
        RightWidth = rightWidth;
        return this;
    }

    public WallPieCutCornerCabinetBuilder WithRightDepth(Dimension rightDepth) {
        RightDepth = rightDepth;
        return this;
    }

    public WallPieCutCornerCabinetBuilder WithAdjustableShelves(int adjustableShelves) {
        AdjustableShelves = adjustableShelves;
        return this;
    }

    public WallPieCutCornerCabinetBuilder WithHingeSide(HingeSide hingeSide) {
        HingeSide = hingeSide;
        return this;
    }

    public WallPieCutCornerCabinetBuilder WithExtendedDoor(Dimension extendDown) {
        ExtendDown = extendDown;
        return this;
    }

    public override WallPieCutCornerCabinet Build() {
        var cabinet = WallPieCutCornerCabinet.Create(Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, MDFDoorOptions, EdgeBandingColor, RightSide, LeftSide, Comment, RightWidth, RightDepth, AdjustableShelves, HingeSide, ExtendDown);
        return cabinet;
    }

}
