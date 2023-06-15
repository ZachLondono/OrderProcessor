using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

internal class BlindBaseCabinetBuilder : CabinetBuilder<BlindBaseCabinet> {

    public BlindCabinetDoors Doors { get; private set; }
    public HorizontalDrawerBank Drawers { get; private set; }
    public int AdjustableShelves { get; private set; }
    public ShelfDepth ShelfDepth { get; private set; }
    public BlindSide BlindSide { get; private set; }
    public Dimension BlindWidth { get; private set; }
    public ToeType ToeType { get; private set; }
    public CabinetDrawerBoxOptions BoxOptions { get; private set; }

    public BlindBaseCabinetBuilder() {
        Doors = new();
        AdjustableShelves = 0;
        ShelfDepth = ShelfDepth.Default;
        BlindSide = BlindSide.Left;
        BlindWidth = Dimension.Zero;
        ToeType = ToeType.NoToe;
        Drawers = new() {
            FaceHeight = Dimension.Zero,
            Quantity = 0
        };
        BoxOptions = new(CabinetDrawerBoxMaterial.FingerJointBirch, DrawerSlideType.UnderMount);
    }

    public BlindBaseCabinetBuilder WithDoors(BlindCabinetDoors doors) {
        Doors = doors;
        return this;
    }

    public BlindBaseCabinetBuilder WithDrawers(HorizontalDrawerBank drawers) {
        Drawers = drawers;
        return this;
    }

    public BlindBaseCabinetBuilder WithAdjustableShelves(int adjustableShelves) {
        AdjustableShelves = adjustableShelves;
        return this;
    }

    public BlindBaseCabinetBuilder WithShelfDepth(ShelfDepth shelfDepth) {
        ShelfDepth = shelfDepth;
        return this;
    }

    public BlindBaseCabinetBuilder WithBlindSide(BlindSide blindSide) {
        BlindSide = blindSide;
        return this;
    }

    public BlindBaseCabinetBuilder WithBlindWidth(Dimension blindWidth) {
        BlindWidth = blindWidth;
        return this;
    }

    public BlindBaseCabinetBuilder WithToeType(ToeType toeType) {
        ToeType = toeType;
        return this;
    }

    public BlindBaseCabinetBuilder WithBoxOptions(CabinetDrawerBoxOptions boxOptions) {
        BoxOptions = boxOptions;
        return this;
    }

    public override BlindBaseCabinet Build() {
        var cabinet = BlindBaseCabinet.Create(Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, SlabDoorMaterial, MDFDoorOptions, EdgeBandingColor, RightSideType, LeftSideType, Comment, Doors, BlindSide, BlindWidth, AdjustableShelves, ShelfDepth, Drawers, ToeType, BoxOptions);
        return cabinet;
    }

}