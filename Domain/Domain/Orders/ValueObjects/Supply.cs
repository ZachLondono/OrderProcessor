using Domain.ValueObjects;

namespace Domain.Orders.ValueObjects;

public record Supply(int Qty, string Name, SupplyType Type) {

    public const double FOUR_HINGE_MAX = 10000;
    public const double THREE_HINGE_MAX = 1830;
    public const double TWO_HINGE_MAX = 1068;

    public static Supply DoorPull(int qty) => new(qty, "Standard Door Pull", SupplyType.Pulls);

    public static Supply DrawerPull(int qty) => new(qty, "Standard Drawer Pull", SupplyType.Pulls);

    public static Supply CabinetLeveler(int qty) => new(qty, "Cabinet Leveler", SupplyType.CabinetLegs);

    public static IEnumerable<Supply> StandardHinge(int qty) => new Supply[2] { new(qty, "Hinge, 125", SupplyType.Hinges), new(qty, "Hinge Plate", SupplyType.Hinges) };

    public static IEnumerable<Supply> StandardHinge(Dimension doorHeight, int doorQty) {

        int hingeQty = doorHeight.AsMillimeters() switch {
            > FOUR_HINGE_MAX => 5,
            > THREE_HINGE_MAX => 4,
            > TWO_HINGE_MAX => 3,
            _ => 2
        };

        return StandardHinge(doorQty * hingeQty);

    }

    public static IEnumerable<Supply> BlindCornerHinge(int qty) => new Supply[2] { new(qty, "Hinge, Blind Corner", SupplyType.Hinges), new(qty, "Hinge Plate", SupplyType.Hinges) };

    public static IEnumerable<Supply> CrossCornerHinge(int qty) => new Supply[2] { new(qty, "Hinge, Cross Corner", SupplyType.Hinges), new(qty, "Hinge Plate", SupplyType.Hinges) };

    public static IEnumerable<Supply> CrossCornerHinge(Dimension doorHeight, int doorQty) {

        int hingeQty = doorHeight.AsMillimeters() switch {
            > FOUR_HINGE_MAX => 5,
            > THREE_HINGE_MAX => 4,
            > TWO_HINGE_MAX => 3,
            _ => 2
        };

        return CrossCornerHinge(doorQty * hingeQty);

    }

    public static Supply PullOutBlock(int qty) => new(qty, "Pullout Block", SupplyType.Miscellaneous);

    public static Supply LockingShelfPeg(int qty) => new(qty, "Shelf Peg, Locking", SupplyType.ShelfPins);

    public static Supply UndermountSlide(int qty, Dimension length) => new(qty, $"Undermount {length.AsMillimeters():N0} - {length.AsInches():N0}\"", SupplyType.DrawerSlides);

    public static Supply SidemountSlide(int qty, Dimension length) => new(qty, $"Sidemount {length.AsMillimeters():N0} - {length.AsInches():N0}\"", SupplyType.DrawerSlides);

    public static Supply LazySuzan(int qty) => new(qty, "Pie Cut Lazy Susan", SupplyType.Miscellaneous);

    public static Supply SingleTrashPullout(int qty) => new(qty, "Single Trash Pullout", SupplyType.Miscellaneous);

    public static Supply DoubleTrashPullout(int qty) => new(qty, "Double Trash Pullout", SupplyType.Miscellaneous);

}
