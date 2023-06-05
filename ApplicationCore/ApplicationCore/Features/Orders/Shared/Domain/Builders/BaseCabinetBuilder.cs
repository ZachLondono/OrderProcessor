using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

internal class BaseCabinetBuilder : CabinetBuilder<BaseCabinet> {

    public BaseCabinetDoors Doors { get; private set; }
    public ToeType ToeType { get; private set; }
    public HorizontalDrawerBank Drawers { get; private set; }
    public BaseCabinetInside Inside { get; private set; }
    public CabinetDrawerBoxOptions BoxOptions { get; private set; }

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

    public override BaseCabinet Build() {
        var cabinet = BaseCabinet.Create(Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, SlabDoorMaterial, MDFDoorOptions, EdgeBandingColor, RightSideType, LeftSideType, Comment, Doors, ToeType, Drawers, Inside, BoxOptions);
        return cabinet;
    }

}
