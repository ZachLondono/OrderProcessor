using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.ValueObjects;

namespace Domain.Orders.Builders;

public class BaseCabinetBuilder : CabinetBuilder<BaseCabinet> {

    public BaseCabinetDoors Doors { get; private set; }
    public ToeType ToeType { get; private set; }
    public HorizontalDrawerBank Drawers { get; private set; }
    public BaseCabinetInside Inside { get; private set; }
    public CabinetDrawerBoxOptions BoxOptions { get; private set; }
    public bool IsGarage { get; private set; }
    public CabinetBaseNotch? BaseNotch { get; private set; }

    public BaseCabinetBuilder() {
        Inside = new();
        Doors = new();
        ToeType = ToeType.NoToe;
        Drawers = new() {
            Quantity = 0,
            FaceHeight = Dimension.Zero
        };
        BoxOptions = new(CabinetDrawerBoxMaterial.FingerJointBirch, DrawerSlideType.Unknown);
    }

    public BaseCabinetBuilder WithDoors(BaseCabinetDoors doors) {
        Doors = doors;
        return this;
    }
    public BaseCabinetBuilder WithToeType(ToeType toeType) {
        ToeType = toeType;
        return this;
    }
    public BaseCabinetBuilder WithDrawers(HorizontalDrawerBank drawers) {
        Drawers = drawers;
        return this;
    }
    public BaseCabinetBuilder WithInside(BaseCabinetInside inside) {
        Inside = inside;
        return this;
    }

    public BaseCabinetBuilder WithBoxOptions(CabinetDrawerBoxOptions boxOptions) {
        BoxOptions = boxOptions;
        return this;
    }

    public BaseCabinetBuilder WithIsGarage(bool isGarage) {
        IsGarage = isGarage;
        return this;
    }

    public BaseCabinetBuilder WithBaseNotch(CabinetBaseNotch baseNotch) {
        BaseNotch = baseNotch;
        return this;
    }

    public override BaseCabinet Build() {
        var cabinet = BaseCabinet.Create(Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, SlabDoorMaterial, MDFDoorOptions, EdgeBandingColor, RightSideType, LeftSideType, Comment, Doors, ToeType, Drawers, Inside, BoxOptions, BaseNotch);
        cabinet.IsGarage = IsGarage;
        cabinet.ProductionNotes = ProductionNotes;
        return cabinet;
    }

}
