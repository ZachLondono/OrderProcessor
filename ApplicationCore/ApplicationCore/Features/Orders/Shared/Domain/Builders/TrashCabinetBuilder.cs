using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Cabinets;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

internal class TrashCabinetBuilder : CabinetBuilder<TrashCabinet> {

    public Dimension DrawerFaceHeight { get; private set; }
    public TrashPulloutConfiguration TrashPulloutConfiguration { get; private set; }
    public ToeType ToeType { get; private set; }
    public CabinetDrawerBoxOptions BoxOptions { get; private set; }

    public TrashCabinetBuilder() {
        DrawerFaceHeight = Dimension.FromMillimeters(157);
        TrashPulloutConfiguration = TrashPulloutConfiguration.OneCan;
        ToeType = ToeType.LegLevelers;
        BoxOptions = new(CabinetDrawerBoxMaterial.FingerJointBirch, DrawerSlideType.UnderMount);
    }

    public TrashCabinetBuilder WithDrawerFaceHeight(Dimension drawerFaceHeight) {
        DrawerFaceHeight = drawerFaceHeight;
        return this;
    }

    public TrashCabinetBuilder WithToeType(ToeType toeType) {
        ToeType = toeType;
        return this;
    }

    public TrashCabinetBuilder WithTrashPulloutConfiguration(TrashPulloutConfiguration trashPulloutConfiguration) {
        TrashPulloutConfiguration = trashPulloutConfiguration;
        return this;
    }

    public TrashCabinetBuilder WithBoxOptions(CabinetDrawerBoxOptions boxOptions) {
        BoxOptions = boxOptions;
        return this;
    }

    public override TrashCabinet Build() {
        var cabinet = TrashCabinet.Create(Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, SlabDoorMaterial, MDFDoorOptions, EdgeBandingColor, RightSideType, LeftSideType, Comment, DrawerFaceHeight, TrashPulloutConfiguration, BoxOptions, ToeType);
        cabinet.ProductionNotes = ProductionNotes;
        return cabinet;
    }

}