using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Cabinets;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

internal class BaseDiagonalCornerCabinetBuilder : CabinetBuilder<BaseDiagonalCornerCabinet> {

    public Dimension RightWidth { get; private set; }
    public Dimension RightDepth { get; private set; }
    public ToeType ToeType { get; private set; }
    public HingeSide HingeSide { get; private set; }
    public int DoorQty { get; private set; }
    public int AdjustableShelves { get; private set; }
    public bool IsGarage { get; private set; }

    public BaseDiagonalCornerCabinetBuilder() {
        RightWidth = Dimension.Zero;
        RightDepth = Dimension.Zero;
        ToeType = ToeType.NoToe;
        HingeSide = HingeSide.Left;
        DoorQty = 1;
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

    public BaseDiagonalCornerCabinetBuilder WithToeType(ToeType toeType) {
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

    public BaseDiagonalCornerCabinetBuilder WithAdjustableShelves(int adjustableShelves) {
        AdjustableShelves = adjustableShelves;
        return this;
    }

    public BaseDiagonalCornerCabinetBuilder WithIsGarage(bool isGarage) {
        IsGarage = isGarage;
        return this;
    }

    public override BaseDiagonalCornerCabinet Build() {
        var cabinet = BaseDiagonalCornerCabinet.Create(Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, SlabDoorMaterial, MDFDoorOptions, EdgeBandingColor, RightSideType, LeftSideType, Comment, RightWidth, RightDepth, ToeType, AdjustableShelves, HingeSide, DoorQty);
        cabinet.IsGarage = IsGarage;
        cabinet.ProductionNotes = ProductionNotes;
        return cabinet;
    }

}
