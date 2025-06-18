namespace Domain.Orders.Enums;

public enum DoorType {

    Door,

    // Drawer fronts do not get hinges and hardware is attached to the center of the drawer front or as a pair, centered vertically
    DrawerFront,

    // Hamper doors will have hinges at the bottom and hardware centered at the top
    HamperDoor,

    // Applied panels do not get hinges or hardware, they are attached to a cabinet or appliance for decoration
    AppliedPanel

}
