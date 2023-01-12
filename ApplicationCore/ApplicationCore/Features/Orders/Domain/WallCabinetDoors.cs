using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Domain;

public record WallCabinetDoors {

    public int Quantity { get; init; }
    public HingeSide HingeSide { get; init; } = HingeSide.NotApplicable;
    public MDFDoorOptions? MDFOptions { get; init; }
    public Dimension ExtendDown { get; init; }

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
