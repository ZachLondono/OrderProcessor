namespace ApplicationCore.Features.Orders.Domain.ValueObjects;

// TODO: Store cab/item number in part
public record DrawerBoxPart(DrawerBoxPartType Type, int Qty, Dimension Width, Dimension Length, Guid MaterialId, string Comment);

public enum DrawerBoxPartType {
    Unknown,
    Front,
    Back,
    Side,
    Bottom,
    BackLeft,
    BackRight,
    BackCenter,
    SideCenter
}
