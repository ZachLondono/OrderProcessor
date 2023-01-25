using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.Builders;

internal class WallCabinetBuilder : CabinetBuilder<WallCabinet> {

    public WallCabinetDoors Doors { get; private set; }
    public WallCabinetInside Inside { get; private set; }
    public bool FinishBottom { get; private set; }
    public CabinetDoorGaps DoorGaps { get; private set; }

    public WallCabinetBuilder() : base() {
        Doors = new();
        Inside = new();
        FinishBottom = false;
        DoorGaps = new() {
            TopGap = Dimension.FromMillimeters(3),
            BottomGap = Dimension.Zero,
            EdgeReveal = Dimension.FromMillimeters(2),
            HorizontalGap = Dimension.FromMillimeters(3),
            VerticalGap = Dimension.FromMillimeters(3),
        };
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
        var cabinet = WallCabinet.Create(Qty, UnitPrice, Room, Assembled, Height, Width, Depth, BoxMaterial, FinishMaterial, EdgeBandingColor, RightSide, LeftSide, Doors, Inside, FinishBottom);
        cabinet.DoorGaps = DoorGaps;
        return cabinet;
    }

}
