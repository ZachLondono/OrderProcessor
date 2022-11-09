namespace ApplicationCore.Features.Orders.Domain.ValueObjects;

// TODO: Store cab/item number in part
public record DrawerBoxPart(DrawerBoxPartType Type, int Qty, Dimension Width, Dimension Length, Dimension Thickness, string MaterialName, string Comment);

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
