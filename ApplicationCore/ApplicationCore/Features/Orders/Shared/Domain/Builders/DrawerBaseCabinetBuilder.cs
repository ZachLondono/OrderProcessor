using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

internal class DrawerBaseCabinetBuilder : CabinetBuilder<DrawerBaseCabinet> {

    public IToeType ToeType { get; private set; }
    public MDFDoorOptions? Fronts { get; private set; }
    public VerticalDrawerBank Drawers { get; private set; }

    public DrawerBaseCabinetBuilder() {
        ToeType = new NoToe();
        Fronts = null;
        Drawers = new() {
            BoxMaterial = CabinetDrawerBoxMaterial.FingerJointBirch,
            FaceHeights = Enumerable.Empty<Dimension>(),
            SlideType = DrawerSlideType.UnderMount
        };
    }

    public DrawerBaseCabinetBuilder WithToeType(IToeType toeType) {
        ToeType = toeType;
        return this;
    }

    public DrawerBaseCabinetBuilder WithFronts(MDFDoorOptions? fronts) {
        Fronts = fronts;
        return this;
    }

    public DrawerBaseCabinetBuilder WithDrawers(VerticalDrawerBank drawers) {
        Drawers = drawers;
        return this;
    }

    public override DrawerBaseCabinet Build() {
        return DrawerBaseCabinet.Create(Qty, UnitPrice, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, EdgeBandingColor, RightSide, LeftSide, ToeType, Drawers, Fronts);
    }
}
