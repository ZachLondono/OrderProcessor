using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

// TODO: Store cab/item number in part
public record DrawerBoxPart(DrawerBoxPartType Type, int Qty, Dimension Width, Dimension Length, string Material, string Comment);

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
