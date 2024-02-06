using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace Domain.Orders.Builders;

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
        var cabinet = BasePieCutCornerCabinet.Create(Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, SlabDoorMaterial, MDFDoorOptions, EdgeBandingColor, RightSideType, LeftSideType, Comment, RightWidth, RightDepth, ToeType, AdjustableShelves, HingeSide);
        cabinet.ProductionNotes = ProductionNotes;
        return cabinet;
    }

}