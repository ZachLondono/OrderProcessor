using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

internal class SinkCabinetBuilder : CabinetBuilder<SinkCabinet> {

    public ToeType ToeType { get; private set; }
    public HingeSide HingeSide { get; private set; }
    public int DoorQty { get; private set; }
    public int FalseDrawerQty { get; private set; }
    public Dimension DrawerFaceHeight { get; private set; }
    public int AdjustableShelves { get; private set; }
    public ShelfDepth ShelfDepth { get; private set; }
    public RollOutOptions RollOutBoxes { get; private set; }

    public SinkCabinetBuilder() {
        ToeType = ToeType.NoToe;
        HingeSide = HingeSide.Left;
        DoorQty = 0;
        FalseDrawerQty = 0;
        DrawerFaceHeight = Dimension.Zero;
        AdjustableShelves = 0;
        ShelfDepth = ShelfDepth.Default;
        RollOutBoxes = new(Array.Empty<Dimension>(), false, RollOutBlockPosition.Both, DrawerSlideType.UnderMount, CabinetDrawerBoxMaterial.FingerJointBirch);
    }

    public SinkCabinetBuilder WithToeType(ToeType toeType) {
        ToeType = toeType;
        return this;
    }

    public SinkCabinetBuilder WithHingeSide(HingeSide hingeSide) {
        HingeSide = hingeSide;
        return this;
    }

    public SinkCabinetBuilder WithDoorQty(int doorQty) {
        DoorQty = doorQty;
        return this;
    }

    public SinkCabinetBuilder WithFalseDrawerQty(int falseDrawerQty) {
        FalseDrawerQty = falseDrawerQty;
        return this;
    }

    public SinkCabinetBuilder WithDrawerFaceHeight(Dimension drawerFaceHeight) {
        DrawerFaceHeight = drawerFaceHeight;
        return this;
    }

    public SinkCabinetBuilder WithAdjustableShelves(int adjustableShelves) {
        AdjustableShelves = adjustableShelves;
        return this;
    }

    public SinkCabinetBuilder WithShelfDepth(ShelfDepth shelfDepth) {
        ShelfDepth = shelfDepth;
        return this;
    }

    public SinkCabinetBuilder WithRollOutBoxes(RollOutOptions rollOutBoxes) {
        RollOutBoxes = rollOutBoxes;
        return this;
    }

    public override SinkCabinet Build() {
        var cabinet = SinkCabinet.Create(Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, MDFDoorOptions, EdgeBandingColor, RightSide, LeftSide, Comment, ToeType, HingeSide, DoorQty, FalseDrawerQty, DrawerFaceHeight, AdjustableShelves, ShelfDepth, RollOutBoxes);
        return cabinet;
    }

}
