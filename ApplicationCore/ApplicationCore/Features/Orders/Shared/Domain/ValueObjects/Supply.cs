using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record Supply(int Qty, string Name) {

    public const double FOUR_HINGE_MAX = 10000;
    public const double THREE_HINGE_MAX = 1830;
    public const double TWO_HINGE_MAX = 1068;

    public static Supply DoorPull(int qty) => new(qty, "Standard Door Pull");

    public static Supply DrawerPull(int qty) => new(qty, "Standard Drawer Pull");

    public static Supply CabinetLeveler(int qty) => new(qty, "Cabinet Leveler");

    public static IEnumerable<Supply> StandardHinge(int qty) => new Supply[2] { new(qty, "Hinge, 125"), new(qty, "Hinge Plate") };

    public static IEnumerable<Supply> StandardHinge(Dimension doorHeight, int doorQty) {

        int hingeQty = doorHeight.AsMillimeters() switch {
            > FOUR_HINGE_MAX => 5,
            > THREE_HINGE_MAX => 4,
            > TWO_HINGE_MAX => 3,
            _ => 2
        };

        return StandardHinge(doorQty * hingeQty);

    }

    public static IEnumerable<Supply> BlindCornerHinge(int qty) => new Supply[2] { new(qty, "Hinge, Blind Corner"), new(qty, "Hinge Plate") };

    public static IEnumerable<Supply> CrossCornerHinge(int qty) => new Supply[2] { new(qty, "Hinge, Cross Corner"), new(qty, "Hinge Plate") };

    public static IEnumerable<Supply> CrossCornerHinge(Dimension doorHeight, int doorQty) {

        int hingeQty = doorHeight.AsMillimeters() switch {
            > FOUR_HINGE_MAX => 5,
            > THREE_HINGE_MAX => 4,
            > TWO_HINGE_MAX => 3,
            _ => 2
        };

        return CrossCornerHinge(doorQty * hingeQty);

    }

    public static Supply PullOutBlock(int qty) => new(qty, "Pullout Block");

    public static Supply LockingShelfPeg(int qty) => new(qty, "Shelf Peg, Locking");

    public static Supply UndermountSlide(int qty, Dimension length) => new(qty, $"Tandem {length.AsMillimeters():N0} - {length.AsInches():N0}\"");

    public static Supply SidemountSlide(int qty, Dimension length) => new(qty, $"Sidemount {length.AsMillimeters():N0} - {length.AsInches():N0}\"");

    public static Supply LazySuzan(int qty) => new(qty, "Pie Cut Lazy Susan");

    public static Supply SingleTrashPullout(int qty) => new(qty, "Single Trash Pullout");

    public static Supply DoubleTrashPullout(int qty) => new(qty, "Double Trash Pullout");

}
