using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain;

public record RollOutOptions(Dimension[] Positions, bool ScoopFront, RollOutBlockPosition Blocks, DrawerSlideType SlideType, CabinetDrawerBoxMaterial Material) { };