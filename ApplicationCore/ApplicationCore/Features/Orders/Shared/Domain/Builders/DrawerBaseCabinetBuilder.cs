using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

internal class DrawerBaseCabinetBuilder : CabinetBuilder<DrawerBaseCabinet> {

    public ToeType ToeType { get; private set; }
    public VerticalDrawerBank Drawers { get; private set; }

    public DrawerBaseCabinetBuilder() {
        ToeType = ToeType.NoToe;
        Drawers = new() {
            BoxMaterial = CabinetDrawerBoxMaterial.FingerJointBirch,
            FaceHeights = Enumerable.Empty<Dimension>(),
            SlideType = DrawerSlideType.UnderMount
        };
    }

    public DrawerBaseCabinetBuilder WithToeType(ToeType toeType) {
        ToeType = toeType;
        return this;
    }

    public DrawerBaseCabinetBuilder WithDrawers(VerticalDrawerBank drawers) {
        Drawers = drawers;
        return this;
    }

    public override DrawerBaseCabinet Build() {
        var cabinet = DrawerBaseCabinet.Create(Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, MDFDoorOptions, EdgeBandingColor, RightSide, LeftSide, Comment, ToeType, Drawers);
        return cabinet;
    }
}
