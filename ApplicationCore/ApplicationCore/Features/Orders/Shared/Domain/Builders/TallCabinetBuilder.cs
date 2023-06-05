using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

internal class TallCabinetBuilder : CabinetBuilder<TallCabinet> {

    public TallCabinetDoors Doors { get; private set; }
    public ToeType ToeType { get; private set; }
    public TallCabinetInside Inside { get; private set; }
    public CabinetDrawerBoxOptions BoxOptions { get; private set; }

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

    public TallCabinetBuilder WithBoxOptions(CabinetDrawerBoxOptions boxOptions) {
        BoxOptions = boxOptions;
        return this;
    }

    public override TallCabinet Build() {
        var cabinet = TallCabinet.Create(Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, SlabDoorMaterial, MDFDoorOptions, EdgeBandingColor, RightSideType, LeftSideType, Comment, Doors, ToeType, Inside, BoxOptions);
        return cabinet;
    }
}
