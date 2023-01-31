using ApplicationCore.Features.Orders.Shared.Domain.Enums;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record BlindCabinetDoors {

    public int Quantity { get; init; }
    public HingeSide HingeSide { get; init; }

    public BlindCabinetDoors(HingeSide hingeSide = HingeSide.NotApplicable) {
        
        if (hingeSide == HingeSide.NotApplicable) {
            Quantity = 2;
        } else { 
            Quantity = 1;
        }

        HingeSide = hingeSide;

    }

}
