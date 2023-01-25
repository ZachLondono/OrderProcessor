using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

internal class TallCabinetBuilder : CabinetBuilder<TallCabinet> {

    public TallCabinetDoors Doors { get; private set; }
    public IToeType ToeType { get; private set; }
    public TallCabinetInside Inside { get; private set; }
    public CabinetDoorGaps DoorGaps { get; private set; }

    public TallCabinetBuilder() {
        Doors = new();
        ToeType = new NoToe();
        Inside = new();
        DoorGaps = new() {
            TopGap = Dimension.FromMillimeters(3),
            BottomGap = Dimension.Zero,
            EdgeReveal = Dimension.FromMillimeters(2),
            HorizontalGap = Dimension.FromMillimeters(3),
            VerticalGap = Dimension.FromMillimeters(3),
        };
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
        var cabinet = TallCabinet.Create(Qty, UnitPrice, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, EdgeBandingColor, RightSide, LeftSide, Doors, ToeType, Inside);
        cabinet.DoorGaps = DoorGaps;
        return cabinet;
    }
}
