using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

internal class BaseCabinetBuilder : CabinetBuilder<BaseCabinet> {

    public BaseCabinetDoors Doors { get; private set; }
    public IToeType ToeType { get; private set; }
    public HorizontalDrawerBank Drawers { get; private set; }
    public BaseCabinetInside Inside { get; private set; }

    public BaseCabinetBuilder() {
        Inside = new();
        Doors = new();
        ToeType = new NoToe();
        Drawers = new() {
            Quantity = 0,
            BoxMaterial = CabinetDrawerBoxMaterial.FingerJointBirch,
            FaceHeight = Dimension.Zero,
            SlideType = DrawerSlideType.UnderMount
        };
    }

    public BaseCabinetBuilder WithDoors(BaseCabinetDoors doors) {
        Doors = doors;
        return this;
    }
    public BaseCabinetBuilder WithToeType(IToeType toeType) {
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

    public override BaseCabinet Build() {
        var cabinet = BaseCabinet.Create(Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, MDFDoorOptions, EdgeBandingColor, RightSide, LeftSide, Doors, ToeType, Drawers, Inside);
        return cabinet;
    }

}
