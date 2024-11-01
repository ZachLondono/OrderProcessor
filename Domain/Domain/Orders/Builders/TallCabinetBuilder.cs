using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products.Cabinets;

namespace Domain.Orders.Builders;

public class TallCabinetBuilder : CabinetBuilder<TallCabinet> {

    public TallCabinetDoors Doors { get; private set; }
    public ToeType ToeType { get; private set; }
    public TallCabinetInside Inside { get; private set; }
    public CabinetDrawerBoxOptions? BoxOptions { get; private set; }
    public bool IsGarage { get; private set; }
    public CabinetBaseNotch? BaseNotch { get; private set; }

    public TallCabinetBuilder() {
        Doors = new();
        ToeType = ToeType.NoToe;
        Inside = new();
        BoxOptions = new(CabinetDrawerBoxMaterial.FingerJointBirch, DrawerSlideType.UnderMount);
    }

    public TallCabinetBuilder WithDoors(TallCabinetDoors doors) {
        Doors = doors;
        return this;
    }

    public TallCabinetBuilder WithToeType(ToeType toeType) {
        ToeType = toeType;
        return this;
    }

    public TallCabinetBuilder WithInside(TallCabinetInside inside) {
        Inside = inside;
        return this;
    }

    public TallCabinetBuilder WithBoxOptions(CabinetDrawerBoxOptions? boxOptions) {
        BoxOptions = boxOptions;
        return this;
    }

    public TallCabinetBuilder WithIsGarage(bool isGarage) {
        IsGarage = isGarage;
        return this;
    }

    public TallCabinetBuilder WithBaseNotch(CabinetBaseNotch? baseNotch) {
        BaseNotch = baseNotch;
        return this;
    }

    public override TallCabinet Build() {
        var cabinet = TallCabinet.Create(Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, SlabDoorMaterial, MDFDoorOptions, EdgeBandingColor, RightSideType, LeftSideType, Comment, Doors, ToeType, Inside, BoxOptions, BaseNotch);
        cabinet.IsGarage = IsGarage;
        cabinet.ProductionNotes = ProductionNotes;
        return cabinet;
    }
}
