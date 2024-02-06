using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record RollOutOptions(Dimension[] Positions, bool ScoopFront, RollOutBlockPosition Blocks) {

    public int Qty => Positions.Length;

    public bool Any() => Positions.Any();

};