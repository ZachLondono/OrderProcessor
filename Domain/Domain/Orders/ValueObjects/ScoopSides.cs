using Domain.ValueObjects;

namespace Domain.Orders.ValueObjects;

public record ScoopSides(Dimension Depth, Dimension FromFront, Dimension FromBack);