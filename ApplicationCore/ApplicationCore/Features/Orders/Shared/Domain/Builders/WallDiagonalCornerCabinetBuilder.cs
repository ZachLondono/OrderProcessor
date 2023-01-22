using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

internal class WallDiagonalCornerCabinetBuilder : CabinetBuilder<WallDiagonalCornerCabinet> {

    public Dimension RightWidth { get; private set; }
    public Dimension RightDepth { get; private set; }
    public HingeSide HingeSide { get; private set; }
    public int DoorQty { get; private set; }
    public MDFDoorOptions? MDFOptions { get; private set; }
    public int AdjustableShelves { get; private set; }

    public WallDiagonalCornerCabinetBuilder() {
        RightWidth = Dimension.Zero;
        RightDepth = Dimension.Zero;
        HingeSide = HingeSide.Left;
        DoorQty = 0;
        MDFOptions = null;
        AdjustableShelves = 0;
    }

    public WallDiagonalCornerCabinetBuilder WithRightWidth(Dimension rightWidth) {
        RightWidth = rightWidth;
        return this;
    }

    public WallDiagonalCornerCabinetBuilder WithRightDepth(Dimension rightDepth) {
        RightDepth = rightDepth;
        return this;
    }

    public WallDiagonalCornerCabinetBuilder WithHingeSide(HingeSide hingeSide) {
        HingeSide = hingeSide;
        return this;
    }

    public WallDiagonalCornerCabinetBuilder WithDoorQty(int doorQty) {
        DoorQty = doorQty;
        return this;
    }

    public WallDiagonalCornerCabinetBuilder WithMDFOptions(MDFDoorOptions? mdfOptions) {
        MDFOptions = mdfOptions;
        return this;
    }

    public WallDiagonalCornerCabinetBuilder WithAdjustableShelves(int adjustableShelves) {
        AdjustableShelves = adjustableShelves;
        return this;
    }

    public override WallDiagonalCornerCabinet Build() {
        return WallDiagonalCornerCabinet.Create(Qty, UnitPrice, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, EdgeBandingColor, RightSide, LeftSide, RightWidth, RightDepth, AdjustableShelves, HingeSide, DoorQty, MDFOptions);
    }

}
