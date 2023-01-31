using ApplicationCore.Features.Orders.Shared.Domain.Enums;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record BaseCabinetDoors {

    public int Quantity { get; init; }
    public HingeSide HingeSide { get; init; }

    public static BaseCabinetDoors NoDoors() => new() {
        Quantity = 0,
        HingeSide = HingeSide.NotApplicable
    };

    public BaseCabinetDoors(HingeSide hingeSide = HingeSide.NotApplicable) {
        
        if (hingeSide == HingeSide.NotApplicable) { 
            Quantity = 1;
        } else {
            Quantity = 2;
        }

        HingeSide = hingeSide;
    }

}
