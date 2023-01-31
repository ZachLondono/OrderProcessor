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

    public WallCabinetDoors(HingeSide hingeSide = HingeSide.NotApplicable, Dimension? extendDown = null) {
        if (hingeSide == HingeSide.NotApplicable) {
            Quantity = 2;
        } else {
            Quantity = 1;
        }
        HingeSide = hingeSide;
        ExtendDown = extendDown is null ? Dimension.Zero : (Dimension) extendDown;
    }

}
