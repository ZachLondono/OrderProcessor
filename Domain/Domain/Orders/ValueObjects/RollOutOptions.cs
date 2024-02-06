using Domain.Orders.Enums;
using Domain.ValueObjects;

namespace Domain.Orders.ValueObjects;

public record RollOutOptions(Dimension[] Positions, bool ScoopFront, RollOutBlockPosition Blocks) {

    public int Qty => Positions.Length;

    public bool Any() => Positions.Any();

};