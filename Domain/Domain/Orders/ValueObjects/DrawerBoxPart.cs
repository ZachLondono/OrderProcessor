using Domain.Orders.Enums;
using Domain.ValueObjects;

namespace Domain.Orders.ValueObjects;

// TODO: Store cab/item number in part
public record DovetailDrawerBoxPart(DrawerBoxPartType Type, int Qty, int ProductNumber, Dimension Width, Dimension Length, string Material, string Comment);
