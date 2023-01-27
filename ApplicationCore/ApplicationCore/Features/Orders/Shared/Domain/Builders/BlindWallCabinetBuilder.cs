using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

internal class BlindWallCabinetBuilder : CabinetBuilder<BlindWallCabinet> {

    public BlindCabinetDoors Doors { get; private set; }
    public int AdjustableShelves { get; private set; }
    public BlindSide BlindSide { get; private set; }
    public Dimension BlindWidth { get; private set; }

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

    public override BlindWallCabinet Build() {
        var cabinet = BlindWallCabinet.Create(Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, EdgeBandingColor, RightSide, LeftSide, Doors, BlindSide, BlindWidth, AdjustableShelves);
        return cabinet;
    }

}
