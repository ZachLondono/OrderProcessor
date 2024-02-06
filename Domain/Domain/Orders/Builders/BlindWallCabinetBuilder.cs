using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.ValueObjects;

namespace Domain.Orders.Builders;

public class BlindWallCabinetBuilder : CabinetBuilder<BlindWallCabinet> {

    public BlindCabinetDoors Doors { get; private set; }
    public int AdjustableShelves { get; private set; }
    public BlindSide BlindSide { get; private set; }
    public Dimension BlindWidth { get; private set; }
    public Dimension ExtendDown { get; private set; }

    public BlindWallCabinetBuilder() {
        Doors = new();
        AdjustableShelves = 0;
        BlindSide = BlindSide.Left;
        BlindWidth = Dimension.Zero;
    }

    public BlindWallCabinetBuilder WithDoors(BlindCabinetDoors doors) {
        Doors = doors;
        return this;
    }

    public BlindWallCabinetBuilder WithAdjustableShelves(int adjShelves) {
        AdjustableShelves = adjShelves;
        return this;
    }

    public BlindWallCabinetBuilder WithBlindSide(BlindSide blindSide) {
        BlindSide = blindSide;
        return this;
    }

    public BlindWallCabinetBuilder WithBlindWidth(Dimension blindWidth) {
        BlindWidth = blindWidth;
        return this;
    }

    public BlindWallCabinetBuilder WithExtendedDoor(Dimension extendDown) {
        ExtendDown = extendDown;
        return this;
    }

    public override BlindWallCabinet Build() {
        var cabinet = BlindWallCabinet.Create(Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, SlabDoorMaterial, MDFDoorOptions, EdgeBandingColor, RightSideType, LeftSideType, Comment, Doors, BlindSide, BlindWidth, AdjustableShelves, ExtendDown);
        cabinet.ProductionNotes = ProductionNotes;
        return cabinet;
    }

}
