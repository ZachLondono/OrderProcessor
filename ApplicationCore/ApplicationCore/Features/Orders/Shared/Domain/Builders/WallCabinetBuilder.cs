using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

internal class WallCabinetBuilder : CabinetBuilder<WallCabinet> {

    public WallCabinetDoors Doors { get; private set; }
    public WallCabinetInside Inside { get; private set; }
    public bool FinishBottom { get; private set; }

    public WallCabinetBuilder() : base() {
        Doors = new();
        Inside = new();
        FinishBottom = false;
    }

    public WallCabinetBuilder WithDoors(WallCabinetDoors doors) {
        Doors = doors;
        return this;
    }

    public WallCabinetBuilder WithInside(WallCabinetInside inside) {
        Inside = inside;
        return this;
    }

    public WallCabinetBuilder WithFinishBottom(bool finishBottom) {
        FinishBottom = finishBottom;
        return this;
    }

    public override WallCabinet Build() {
        var cabinet = WallCabinet.Create(Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, EdgeBandingColor, RightSide, LeftSide, Doors, Inside, FinishBottom);
        return cabinet;
    }

}
