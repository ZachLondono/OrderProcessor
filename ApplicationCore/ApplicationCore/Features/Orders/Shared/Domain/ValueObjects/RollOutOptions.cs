using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record RollOutOptions(Dimension[] Positions, bool ScoopFront, RollOutBlockPosition Blocks, DrawerSlideType SlideType, CabinetDrawerBoxMaterial Material) {

    public int Qty => Positions.Length;

    public bool Any() => Positions.Any();

};