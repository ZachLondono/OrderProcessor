using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.ValueObjects;
using Domain.Orders.Entities;

namespace Domain.Orders.Builders;

public class BlindBaseCabinetBuilder : CabinetBuilder<BlindBaseCabinet> {

    public BlindCabinetDoors Doors { get; private set; }
    public HorizontalDrawerBank Drawers { get; private set; }
    public int AdjustableShelves { get; private set; }
    public ShelfDepth ShelfDepth { get; private set; }
    public BlindSide BlindSide { get; private set; }
    public Dimension BlindWidth { get; private set; }
    public ToeType ToeType { get; private set; }
    public CabinetDrawerBoxOptions BoxOptions { get; private set; }
    public bool IsGarage { get; private set; }

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

    public BlindBaseCabinetBuilder WithIsGarage(bool isGarage) {
        IsGarage = isGarage;
        return this;
    }

    public override BlindBaseCabinet Build() {

        var cabinet = BlindBaseCabinet.Create(Qty,
                                              UnitPrice,
                                              ProductNumber,
                                              Room,
                                              Assembled,
                                              Height,
                                              Width,
                                              Depth,
                                              BoxMaterial,
                                              FinishMaterial,
                                              SlabDoorMaterial,
                                              MDFDoorOptions,
                                              EdgeBandingColor,
                                              RightSideType,
                                              LeftSideType,
                                              Comment,
                                              Doors,
                                              BlindSide,
                                              BlindWidth,
                                              AdjustableShelves,
                                              ShelfDepth,
                                              Drawers,
                                              ToeType,
                                              BoxOptions);

        cabinet.IsGarage = IsGarage;

        if (LeftSideType == CabinetSideType.ConfirmatFinished || RightSideType == CabinetSideType.ConfirmatFinished) {
            ProductionNotes.Add("Confirmat finished (Garage finished) sides will not work with non-garage SKUs. Set cabinet sides to unfinished and then change material to the finished material.");
        }

        cabinet.ProductionNotes.AddRange(ProductionNotes.Select(ProductionNote.Create));

        return cabinet;

    }

}