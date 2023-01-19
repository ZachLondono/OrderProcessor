using ApplicationCore.Features.Orders.Shared.Domain.Enums;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record BlindCabinetDoors {

    public int Quantity { get; init; }
    public HingeSide HingeSide { get; init; } = HingeSide.NotApplicable;
    public MDFDoorOptions? MDFOptions { get; init; }

    public BlindCabinetDoors(MDFDoorOptions? mdfOptions = null) {
        Quantity = 2;
        HingeSide = HingeSide.NotApplicable;
        MDFOptions = mdfOptions;
    }

    public BlindCabinetDoors(HingeSide hingeSide, MDFDoorOptions? mdfOptions = null) {
        Quantity = 1;
        HingeSide = hingeSide;
        MDFOptions = mdfOptions;
    }

}
