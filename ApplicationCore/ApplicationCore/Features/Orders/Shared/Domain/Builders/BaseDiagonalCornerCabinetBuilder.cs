using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

internal class BaseDiagonalCornerCabinetBuilder : CabinetBuilder<BaseDiagonalCornerCabinet> {

    public Dimension RightWidth { get; private set; }
    public Dimension RightDepth { get; private set; }
    public IToeType ToeType { get; private set; }
    public HingeSide HingeSide { get; private set; }
    public int DoorQty { get; private set; }
    public MDFDoorOptions? MDFOptions { get; private set; }
    public int AdjustableShelves { get; private set; }

    public BaseDiagonalCornerCabinetBuilder() {
        RightWidth = Dimension.Zero;
        RightDepth = Dimension.Zero;
        ToeType = new NoToe();
        HingeSide = HingeSide.Left;
        DoorQty = 0;
        MDFOptions = null;
        AdjustableShelves = 0;
    }

    public BaseDiagonalCornerCabinetBuilder WithRightWidth(Dimension rightWidth) {
        RightWidth = rightWidth;
        return this;
    }

    public BaseDiagonalCornerCabinetBuilder WithRightDepth(Dimension rightDepth) {
        RightDepth = rightDepth;
        return this;
    }

    public BaseDiagonalCornerCabinetBuilder WithToeType(IToeType toeType) {
        ToeType = toeType;
        return this;
    }

    public BaseDiagonalCornerCabinetBuilder WithHingeSide(HingeSide hingeSide) {
        HingeSide = hingeSide;
        return this;
    }

    public BaseDiagonalCornerCabinetBuilder WithDoorQty(int doorQty) {
        DoorQty = doorQty;
        return this;
    }

    public BaseDiagonalCornerCabinetBuilder WithMDFOptions(MDFDoorOptions? mdfOptions) {
        MDFOptions = mdfOptions;
        return this;
    }

    public BaseDiagonalCornerCabinetBuilder WithAdjustableShelves(int adjustableShelves) {
        AdjustableShelves = adjustableShelves;
        return this;
    }

    public override BaseDiagonalCornerCabinet Build() {
        return BaseDiagonalCornerCabinet.Create(Qty, UnitPrice, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, EdgeBandingColor, RightSide, LeftSide, RightWidth, RightDepth, ToeType, AdjustableShelves, HingeSide, DoorQty, MDFOptions);
    }

}
