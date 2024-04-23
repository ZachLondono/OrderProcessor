using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace Domain.Orders.Entities.Hardware;

public record Supply(Guid Id, int Qty, string Description) {

    public const double FOUR_HINGE_MAX = 10000;
    public const double THREE_HINGE_MAX = 1830;
    public const double TWO_HINGE_MAX = 1068;

    public static Supply DoorPull(int qty) => new(Guid.NewGuid(), qty, "Standard Door Pull");

    public static Supply DrawerPull(int qty) => new(Guid.NewGuid(), qty, "Standard Drawer Pull");

    public static Supply CabinetLeveler(int qty) => new(Guid.NewGuid(), qty, "Cabinet Leveler");

    public static Supply DrawerClips(int qty) => new(Guid.NewGuid(), qty, "Hettich Clips (pair)");

    public static IEnumerable<Supply> StandardHinge(int qty) => new Supply[2] { new(Guid.NewGuid(), qty, "Hinge, 125"), new(Guid.NewGuid(), qty, "Hinge Plate") };

    public static IEnumerable<Supply> StandardHinge(Dimension doorHeight, int doorQty) {

        int hingeQty = doorHeight.AsMillimeters() switch {
            > FOUR_HINGE_MAX => 5,
            > THREE_HINGE_MAX => 4,
            > TWO_HINGE_MAX => 3,
            _ => 2
        };

        return StandardHinge(doorQty * hingeQty);

    }

    public static IEnumerable<Supply> BlindCornerHinge(int qty) => new Supply[2] { new(Guid.NewGuid(), qty, "Hinge, Blind Corner"), new(Guid.NewGuid(), qty, "Hinge Plate") };

    public static IEnumerable<Supply> CrossCornerHinge(int qty) => new Supply[2] { new(Guid.NewGuid(), qty, "Hinge, Cross Corner"), new(Guid.NewGuid(), qty, "Hinge Plate") };

    public static IEnumerable<Supply> CrossCornerHinge(Dimension doorHeight, int doorQty) {

        int hingeQty = doorHeight.AsMillimeters() switch {
            > FOUR_HINGE_MAX => 5,
            > THREE_HINGE_MAX => 4,
            > TWO_HINGE_MAX => 3,
            _ => 2
        };

        return CrossCornerHinge(doorQty * hingeQty);

    }

    public static Supply PullOutBlock(int qty) => new(Guid.NewGuid(), qty, "Pullout Block");

    public static Supply LockingShelfPeg(int qty) => new(Guid.NewGuid(), qty, "Adj. Shelf Peg, Locking");

    public static Supply StraightShelfPeg(int qty) => new(Guid.NewGuid(), qty, "Adj. Shelf Peg, Straight");

    public static Supply RafixCam(int qty) => new(Guid.NewGuid(), qty, "Rafix Cam");

    public static Supply CamBolt(int qty) => new(Guid.NewGuid(), qty, "Cam Bolt");

    public static Supply CamBoltDoubleSided(int qty) => new(Guid.NewGuid(), qty, "Cam Bolt, Double Sided");

    public static Supply HangingRailBracketLH(int qty) => new(Guid.NewGuid(), qty, "Hanging Rail Bracket LH");

    public static Supply HangingRailBracketRH(int qty) => new(Guid.NewGuid(), qty, "Hanging Rail Bracket RH");

    public static Supply LazySuzan(int qty) => new(Guid.NewGuid(), qty, "Pie Cut Lazy Susan");

    public static Supply SingleTrashPullout(int qty) => new(Guid.NewGuid(), qty, "Single Trash Pullout");

    public static Supply DoubleTrashPullout(int qty) => new(Guid.NewGuid(), qty, "Double Trash Pullout");

}
