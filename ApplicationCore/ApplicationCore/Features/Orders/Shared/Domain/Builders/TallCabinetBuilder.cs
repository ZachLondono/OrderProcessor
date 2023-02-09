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
        var cabinet = TallCabinet.Create(Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, MDFDoorOptions, EdgeBandingColor, RightSide, LeftSide, Comment, Doors, ToeType, Inside);
        return cabinet;
    }
}
