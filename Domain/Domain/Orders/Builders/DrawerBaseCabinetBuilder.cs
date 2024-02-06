using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace Domain.Orders.Builders;

internal class DrawerBaseCabinetBuilder : CabinetBuilder<DrawerBaseCabinet> {

    public ToeType ToeType { get; private set; }
    public VerticalDrawerBank Drawers { get; private set; }
    public CabinetDrawerBoxOptions BoxOptions { get; private set; }
    public bool IsGarage { get; private set; } = false;
    public CabinetBaseNotch? BaseNotch { get; private set; }

    public DrawerBaseCabinetBuilder() {
        ToeType = ToeType.NoToe;
        Drawers = new() {
            FaceHeights = Array.Empty<Dimension>()
        };
        BoxOptions = new(CabinetDrawerBoxMaterial.FingerJointBirch, DrawerSlideType.UnderMount);
    }

    public DrawerBaseCabinetBuilder WithToeType(ToeType toeType) {
        ToeType = toeType;
        return this;
    }

    public DrawerBaseCabinetBuilder WithDrawers(VerticalDrawerBank drawers) {
        Drawers = drawers;
        return this;
    }

    public DrawerBaseCabinetBuilder WithBoxOptions(CabinetDrawerBoxOptions boxOptions) {
        BoxOptions = boxOptions;
        return this;
    }

    public DrawerBaseCabinetBuilder WithIsGarage(bool isGarage) {
        IsGarage = isGarage;
        return this;
    }

    public DrawerBaseCabinetBuilder WithBaseNotch(CabinetBaseNotch baseNotch) {
        BaseNotch = baseNotch;
        return this;
    }

    public override DrawerBaseCabinet Build() {
        var cabinet = DrawerBaseCabinet.Create(Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, SlabDoorMaterial, MDFDoorOptions, EdgeBandingColor, RightSideType, LeftSideType, Comment, ToeType, Drawers, BoxOptions, BaseNotch);
        cabinet.IsGarage = IsGarage;
        cabinet.ProductionNotes = ProductionNotes;
        return cabinet;
    }
}
