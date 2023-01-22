using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

internal class WallPieCutCornerCabinetBuilder : CabinetBuilder<WallPieCutCornerCabinet> {

    public Dimension RightWidth { get; private set; }
    public Dimension RightDepth { get; private set; }
    public int AdjustableShelves { get; private set; }
    public HingeSide HingeSide { get; private set; }
    public MDFDoorOptions? MDFOptions { get; private set; }

    public WallPieCutCornerCabinetBuilder() {
        RightWidth = Dimension.Zero;
        RightDepth = Dimension.Zero;
        AdjustableShelves = 0;
        HingeSide = HingeSide.Left;
        MDFOptions = null;
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

    public WallPieCutCornerCabinetBuilder WithMDFOptions(MDFDoorOptions? mdfOptions) {
        MDFOptions = mdfOptions;
        return this;
    }

    public override WallPieCutCornerCabinet Build() {
        return WallPieCutCornerCabinet.Create(Qty, UnitPrice, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, EdgeBandingColor, RightSide, LeftSide, RightWidth, RightDepth, AdjustableShelves, HingeSide, MDFOptions);
    }

}
