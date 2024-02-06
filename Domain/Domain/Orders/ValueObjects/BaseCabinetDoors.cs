using Domain.Orders.Enums;

namespace Domain.Orders.ValueObjects;

public record BaseCabinetDoors {

    public int Quantity { get; init; }
    public HingeSide HingeSide { get; init; }

    public static BaseCabinetDoors NoDoors() => new() {
        Quantity = 0,
        HingeSide = HingeSide.NotApplicable
    };

    public static BaseCabinetDoors OneDoor(HingeSide hingeSide) {
        if (hingeSide == HingeSide.NotApplicable) {
            throw new ArgumentException("Hinge side must be provided for 1 door cabinet", nameof(hingeSide));
        }

        return new() {
            Quantity = 1,
            HingeSide = hingeSide,
        };
    }

    public static BaseCabinetDoors TwoDoors() => new() {
        Quantity = 2,
        HingeSide = HingeSide.NotApplicable
    };

    public BaseCabinetDoors(HingeSide hingeSide = HingeSide.NotApplicable) {

        if (hingeSide == HingeSide.NotApplicable) {
            Quantity = 2;
        } else {
            Quantity = 1;
        }

        HingeSide = hingeSide;
    }

}
