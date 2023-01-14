using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain;

public record WallCabinetDoors {

    public int Quantity { get; init; }
    public HingeSide HingeSide { get; init; } = HingeSide.NotApplicable;
    public MDFDoorOptions? MDFOptions { get; init; }
    public Dimension ExtendDown { get; init; }

    public static WallCabinetDoors NoDoors() => new() {
        Quantity = 0,
        HingeSide = HingeSide.NotApplicable,
        MDFOptions = null,
        ExtendDown = Dimension.Zero
    };

    public WallCabinetDoors(Dimension? extendDown = null, MDFDoorOptions? mdfOptions = null) {
        Quantity = 2;
        HingeSide = HingeSide.NotApplicable;
        MDFOptions = mdfOptions;
        ExtendDown = extendDown is null ? Dimension.Zero : extendDown;
    }

    public WallCabinetDoors(HingeSide hingeSide, Dimension? extendDown = null, MDFDoorOptions? mdfOptions = null) {
        Quantity = 1;
        HingeSide = hingeSide;
        MDFOptions = mdfOptions;
        ExtendDown = extendDown is null ? Dimension.Zero : extendDown;
    }

}
