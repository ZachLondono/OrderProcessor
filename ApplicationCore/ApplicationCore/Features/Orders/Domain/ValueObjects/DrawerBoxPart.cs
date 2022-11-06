namespace ApplicationCore.Features.Orders.Domain.ValueObjects;

// TODO: Store cab/item number in part
// TODO: Store material thickness in part
public record DrawerBoxPart(DrawerBoxPartType Type, int Qty, Dimension Width, Dimension Length, string MaterialName, string Comment);

public enum DrawerBoxPartType {
    Unkown,
    Front,
    Back,
    Side,
    Bottom,
    BackLeft,
    BackRight,
    BackCenter,
    SideCenter
}
