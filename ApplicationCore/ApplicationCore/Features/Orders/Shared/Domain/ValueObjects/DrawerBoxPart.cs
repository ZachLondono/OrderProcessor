using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

// TODO: Store cab/item number in part
public record DrawerBoxPart(DrawerBoxPartType Type, int Qty, int ProductNumber, Dimension Width, Dimension Length, string Material, string Comment);
