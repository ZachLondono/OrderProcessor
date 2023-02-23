using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

internal class TrashCabinetBuilder : CabinetBuilder<TrashCabinet> {

    public Dimension DrawerFaceHeight { get; private set; }
    public DrawerSlideType SlideType { get; private set; }
    public CabinetDrawerBoxMaterial DrawerBoxMaterial { get; private set; }
    public TrashPulloutConfiguration TrashPulloutConfiguration { get; private set; }
    public ToeType ToeType { get; private set; }

    public TrashCabinetBuilder() {
        DrawerFaceHeight = Dimension.FromMillimeters(157);
        SlideType = DrawerSlideType.UnderMount;
        DrawerBoxMaterial = CabinetDrawerBoxMaterial.SolidBirch;
        TrashPulloutConfiguration = TrashPulloutConfiguration.OneCan;
        ToeType = ToeType.LegLevelers;
    }

    public TrashCabinetBuilder WithDrawerFaceHeight(Dimension drawerFaceHeight) {
        DrawerFaceHeight = drawerFaceHeight;
        return this;
    }

    public TrashCabinetBuilder WithToeType(ToeType toeType) {
        ToeType = toeType;
        return this;
    }

    public TrashCabinetBuilder WithSlideType(DrawerSlideType slideType) {
        SlideType = slideType;
        return this;
    }

    public TrashCabinetBuilder WithTrashPulloutConfiguration(TrashPulloutConfiguration trashPulloutConfiguration) {
        TrashPulloutConfiguration = trashPulloutConfiguration;
        return this;
    }

    public TrashCabinetBuilder WithDrawerBoxMaterial(CabinetDrawerBoxMaterial cabinetDrawerBoxMaterial) {
        DrawerBoxMaterial = cabinetDrawerBoxMaterial;
        return this;
    }

    public override TrashCabinet Build() {
        var cabinet = TrashCabinet.Create(Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, MDFDoorOptions, EdgeBandingColor, RightSide, LeftSide, Comment, DrawerFaceHeight, TrashPulloutConfiguration, SlideType, DrawerBoxMaterial, ToeType);
        return cabinet;
    }

}