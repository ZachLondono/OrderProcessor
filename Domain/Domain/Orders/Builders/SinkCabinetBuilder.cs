using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.ValueObjects;

namespace Domain.Orders.Builders;

public class SinkCabinetBuilder : CabinetBuilder<SinkCabinet> {

    public ToeType ToeType { get; private set; }
    public HingeSide HingeSide { get; private set; }
    public int DoorQty { get; private set; }
    public int FalseDrawerQty { get; private set; }
    public Dimension DrawerFaceHeight { get; private set; }
    public int AdjustableShelves { get; private set; }
    public ShelfDepth ShelfDepth { get; private set; }
    public RollOutOptions RollOutBoxes { get; private set; }
    public CabinetDrawerBoxOptions? BoxOptions { get; private set; }
    public bool TiltFront { get; private set; }
    public ScoopSides? Scoops { get; private set; }

    public SinkCabinetBuilder() {
        ToeType = ToeType.NoToe;
        HingeSide = HingeSide.Left;
        DoorQty = 0;
        FalseDrawerQty = 0;
        DrawerFaceHeight = Dimension.Zero;
        AdjustableShelves = 0;
        ShelfDepth = ShelfDepth.Default;
        RollOutBoxes = new(Array.Empty<Dimension>(), false, RollOutBlockPosition.Both);
        BoxOptions = new(CabinetDrawerBoxMaterial.FingerJointBirch, DrawerSlideType.UnderMount);
        TiltFront = false;
        Scoops = null;
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

    public SinkCabinetBuilder WithBoxOptions(CabinetDrawerBoxOptions? boxOptions) {
        BoxOptions = boxOptions;
        return this;
    }

    public SinkCabinetBuilder WithTiltFront(bool tiltFront) {
        TiltFront = tiltFront;
        return this;
    }

    public SinkCabinetBuilder WithScoops(ScoopSides? scoops) {
        Scoops = scoops;
        return this;
    }

    public override SinkCabinet Build() {
        var cabinet = SinkCabinet.Create(Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, DoorConfiguration, EdgeBandingColor, RightSideType, LeftSideType, Comment, ToeType, HingeSide, DoorQty, FalseDrawerQty, DrawerFaceHeight, AdjustableShelves, ShelfDepth, RollOutBoxes, BoxOptions, TiltFront, Scoops);
        cabinet.ProductionNotes = ProductionNotes;
        return cabinet;
    }

}
