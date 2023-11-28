using ApplicationCore.Features.OpenDoorOrders;

namespace ApplicationCore.Features.DoorOrderSelector;

public class DoorOrderSelection {

    public required DoorOrder DoorOrder { get; init; }
    public required DoorOrderAction Action { get; init; }

}
