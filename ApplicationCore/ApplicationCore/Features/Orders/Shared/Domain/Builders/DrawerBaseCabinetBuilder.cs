using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Cabinets;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

internal class DrawerBaseCabinetBuilder : CabinetBuilder<DrawerBaseCabinet> {

    public ToeType ToeType { get; private set; }
    public VerticalDrawerBank Drawers { get; private set; }
    public CabinetDrawerBoxOptions BoxOptions { get; private set; }

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

    public override DrawerBaseCabinet Build() {
        var cabinet = DrawerBaseCabinet.Create(Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, SlabDoorMaterial, MDFDoorOptions, EdgeBandingColor, RightSideType, LeftSideType, Comment, ToeType, Drawers, BoxOptions);
        return cabinet;
    }
}
