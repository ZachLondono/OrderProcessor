using ApplicationCore.Features.Orders.Shared.Domain.Enums;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record BlindCabinetDoors {

    public int Quantity { get; init; }
    public HingeSide HingeSide { get; init; }

    public static BlindCabinetDoors OneDoor(HingeSide hingeSide) => new(hingeSide);

    public static BlindCabinetDoors TwoDoors() => new(HingeSide.NotApplicable);

    public BlindCabinetDoors(HingeSide hingeSide = HingeSide.NotApplicable) {

        if (hingeSide == HingeSide.NotApplicable) {
            Quantity = 2;
        } else {
            Quantity = 1;
        }

        HingeSide = hingeSide;

    }

    public BlindCabinetDoors(HingeSide hingeSide, int quantity) {

        if (quantity == 1 && hingeSide == HingeSide.NotApplicable) {
            throw new InvalidOperationException("");
        }

        HingeSide = hingeSide;
        Quantity = quantity;
    }

}
