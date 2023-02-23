using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

internal class BasePieCutCornerCabinetBuilder : CabinetBuilder<BasePieCutCornerCabinet> {

    public Dimension RightWidth { get; private set; }
    public Dimension RightDepth { get; private set; }
    public ToeType ToeType { get; private set; }
    public int AdjustableShelves { get; private set; }
    public HingeSide HingeSide { get; private set; }
    public CabinetDoorGaps DoorGaps { get; private set; }

    public BasePieCutCornerCabinetBuilder() {
        RightWidth = Dimension.Zero;
        RightDepth = Dimension.Zero;
        ToeType = ToeType.NoToe;
        AdjustableShelves = 0;
        HingeSide = HingeSide.Left;
    }

    public BasePieCutCornerCabinetBuilder WithRightWidth(Dimension rightWidth) {
        RightWidth = rightWidth;
        return this;
    }

    public BasePieCutCornerCabinetBuilder WithRightDepth(Dimension rightDepth) {
        RightDepth = rightDepth;
        return this;
    }

    public BasePieCutCornerCabinetBuilder WithToeType(ToeType toeType) {
        ToeType = toeType;
        return this;
    }

    public BasePieCutCornerCabinetBuilder WithAdjustableShelves(int adjustableShelves) {
        AdjustableShelves = adjustableShelves;
        return this;
    }

    public BasePieCutCornerCabinetBuilder WithHingeSide(HingeSide hingeSide) {
        HingeSide = hingeSide;
        return this;
    }

    public override BasePieCutCornerCabinet Build() {
        var cabinet = BasePieCutCornerCabinet.Create(Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, MDFDoorOptions, EdgeBandingColor, RightSide, LeftSide, Comment, RightWidth, RightDepth, ToeType, AdjustableShelves, HingeSide);
        return cabinet;
    }

}