using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record TallCabinetDoors {

    public int UpperQuantity { get; init; }
    public int LowerQuantity { get; init; }
    public Dimension LowerDoorHeight { get; init; }
    public HingeSide HingeSide { get; init; } = HingeSide.NotApplicable;

    public static TallCabinetDoors NoDoors() => new() {
        UpperQuantity = 0,
        LowerQuantity = 0,
        LowerDoorHeight = Dimension.Zero,
        HingeSide = HingeSide.NotApplicable
    };

    public static TallCabinetDoors OneDoor(HingeSide hingeSide) {
        if (hingeSide == HingeSide.NotApplicable) {
            throw new ArgumentException("Hinge side must be provided for 1 door cabinet", nameof(hingeSide));
        }

        return new(hingeSide);
    }

    public static TallCabinetDoors TwoDoors() => new(HingeSide.NotApplicable);

    public static TallCabinetDoors TwoDoorsTwoSections(Dimension lowerDoorHeight, HingeSide hingeSide) {
        if (hingeSide == HingeSide.NotApplicable) {
            throw new ArgumentException("Hinge side must be provided for 1 door cabinet", nameof(hingeSide));
        }

        return new(lowerDoorHeight, HingeSide.NotApplicable);
    }

    public static TallCabinetDoors FourDoorsTwoSections(Dimension lowerDoorHeight) => new(lowerDoorHeight, HingeSide.NotApplicable);

    public TallCabinetDoors(HingeSide hingeSide = HingeSide.NotApplicable) {

        if (hingeSide == HingeSide.NotApplicable) {
            UpperQuantity = 0;
            LowerQuantity = 2;
        } else {
            UpperQuantity = 0;
            LowerQuantity = 1;
        }

        LowerDoorHeight = Dimension.Zero;
        HingeSide = hingeSide;

    }

    public TallCabinetDoors(Dimension lowerDoorHeight, HingeSide hingeSide = HingeSide.NotApplicable) {

        if (hingeSide == HingeSide.NotApplicable) {
            UpperQuantity = 2;
            LowerQuantity = 2;
        } else {
            UpperQuantity = 1;
            LowerQuantity = 1;
        }

        LowerDoorHeight = lowerDoorHeight;
        HingeSide = hingeSide;

    }

}
