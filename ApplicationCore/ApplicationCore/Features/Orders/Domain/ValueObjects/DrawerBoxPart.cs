namespace ApplicationCore.Features.Orders.Domain.ValueObjects;

// TODO: store cab/item number in part
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
