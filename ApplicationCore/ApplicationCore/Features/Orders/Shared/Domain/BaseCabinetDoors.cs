using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Shared.Domain;

public record BaseCabinetDoors {

    public int Quantity { get; init; }
    public HingeSide HingeSide { get; init; } = HingeSide.NotApplicable;
    public MDFDoorOptions? MDFOptions { get; init; }

    public static BaseCabinetDoors NoDoors() => new() {
        Quantity = 0,
        HingeSide = HingeSide.NotApplicable,
        MDFOptions = null
    };

    public BaseCabinetDoors(MDFDoorOptions? mdfOptions = null) {
        Quantity = 2;
        HingeSide = HingeSide.NotApplicable;
        MDFOptions = mdfOptions;
    }

    public BaseCabinetDoors(HingeSide hingeSide, MDFDoorOptions? mdfOptions = null) {
        Quantity = 1;
        HingeSide = hingeSide;
        MDFOptions = mdfOptions;
    }

}
