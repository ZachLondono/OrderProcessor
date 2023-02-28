using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record WallCabinetDoors {

    public int Quantity { get; init; }
    public HingeSide HingeSide { get; init; } = HingeSide.NotApplicable;
    public Dimension ExtendDown { get; init; }

    public static WallCabinetDoors NoDoors() => new() {
        Quantity = 0,
        HingeSide = HingeSide.NotApplicable,
        ExtendDown = Dimension.Zero
    };

    public static WallCabinetDoors OneDoor(HingeSide hingeSide) {
        if (hingeSide == HingeSide.NotApplicable) {
            throw new ArgumentException("Hinge side must be provided for 1 door cabinet", nameof(hingeSide));
        }

        return new() {
            Quantity = 1,
            HingeSide = hingeSide,
        };
    }

    public static WallCabinetDoors TwoDoors() => new() {
        Quantity = 2,
        HingeSide = HingeSide.NotApplicable
    };

    public WallCabinetDoors(HingeSide hingeSide = HingeSide.NotApplicable, Dimension? extendDown = null) {
        if (hingeSide == HingeSide.NotApplicable) {
            Quantity = 2;
        } else {
            Quantity = 1;
        }
        HingeSide = hingeSide;
        ExtendDown = extendDown is null ? Dimension.Zero : (Dimension)extendDown;
    }

    public WallCabinetDoors(int quantity, HingeSide hingeSide, Dimension extendDown) {

        if (quantity == 1 && hingeSide == HingeSide.NotApplicable) {
            throw new InvalidOperationException("Hinge side must be specified");
        }

        if (quantity == 2 && hingeSide != HingeSide.NotApplicable) {
            hingeSide = HingeSide.NotApplicable;
        }

        if (extendDown < Dimension.Zero) throw new ArgumentException("Door extend down cannot be negative");

        Quantity = quantity;
        HingeSide = hingeSide;
        ExtendDown = extendDown;

    }

}
