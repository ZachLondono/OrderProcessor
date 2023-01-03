using ApplicationCore.Features.Orders.Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Domain;

public record BaseCabinetDoors {

    public int Quantity { get; init; }
    public HingeSide HingeSide { get; init; } = HingeSide.NotApplicable;
    public MDFDoorOptions? MDFOptions { get; init; }

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
