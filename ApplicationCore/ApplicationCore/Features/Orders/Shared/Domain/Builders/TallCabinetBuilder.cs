using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

internal class TallCabinetBuilder : CabinetBuilder<TallCabinet> {

    public TallCabinetDoors Doors { get; private set; }
    public IToeType ToeType { get; private set; }
    public TallCabinetInside Inside { get; private set; }

    public TallCabinetBuilder() {
        Doors = new();
        ToeType = new NoToe();
        Inside = new();
    }

    public TallCabinetBuilder WithDoors(TallCabinetDoors doors) {
        Doors = doors;
        return this;
    }

    public TallCabinetBuilder WithToeType(IToeType toeType) {
        ToeType = toeType;
        return this;
    }

    public TallCabinetBuilder WithInside(TallCabinetInside inside) {
        Inside = inside;
        return this;
    }

    public override TallCabinet Build() {
        return TallCabinet.Create(Qty, UnitPrice, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, EdgeBandingColor, RightSide, LeftSide, Doors, ToeType, Inside);
    }
}
