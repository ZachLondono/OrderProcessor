using Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record ScoopSides(Dimension Depth, Dimension FromFront, Dimension FromBack);