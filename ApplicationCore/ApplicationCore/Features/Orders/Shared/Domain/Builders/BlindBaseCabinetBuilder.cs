using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

internal class BlindBaseCabinetBuilder : CabinetBuilder<BlindBaseCabinet> {

    public BlindCabinetDoors Doors { get; private set; }
    public HorizontalDrawerBank Drawers { get; private set; }
    public int AdjustableShelves { get; private set; }
    public BlindSide BlindSide { get; private set;  }
    public Dimension BlindWidth { get; private set; }
    public IToeType ToeType { get; private set; }

    public BlindBaseCabinetBuilder() {
        Doors = new();
        AdjustableShelves = 0;
        BlindSide = BlindSide.Left;
        BlindWidth = Dimension.Zero;
        ToeType = new NoToe();
        Drawers = new() {
            BoxMaterial = CabinetDrawerBoxMaterial.FingerJointBirch,
            FaceHeight = Dimension.Zero,
            Quantity = 0,
            SlideType = DrawerSlideType.UnderMount
        };
    }

    public BlindBaseCabinetBuilder WithDoors(BlindCabinetDoors doors) {
        Doors = doors;
        return this;
    }

    public BlindBaseCabinetBuilder WithDrawers (HorizontalDrawerBank drawers) {
        Drawers = drawers;
        return this;
    }

    public BlindBaseCabinetBuilder WithAdjustableShelves (int adjustableShelves) {
        AdjustableShelves = adjustableShelves;
        return this;
    }

    public BlindBaseCabinetBuilder WithBlindSide (BlindSide blindSide) {
        BlindSide = blindSide;
        return this;
    }

    public BlindBaseCabinetBuilder WithBlindWidth (Dimension blindWidth) {
        BlindWidth = blindWidth;
        return this;
    }

    public BlindBaseCabinetBuilder WithToeType (IToeType toeType) {
        ToeType = toeType;
        return this;
    }

    public override BlindBaseCabinet Build() {
        return BlindBaseCabinet.Create(Qty, UnitPrice, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, EdgeBandingColor, RightSide, LeftSide, Doors, Drawers, AdjustableShelves, BlindSide, BlindWidth, ToeType);
    }

}