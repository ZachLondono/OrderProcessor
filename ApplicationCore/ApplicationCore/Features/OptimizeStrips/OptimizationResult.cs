using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.OptimizeStrips;

public record OptimizationResult {
    public required IEnumerable<Dimension> UnplacedParts { get; init; }
    public required IEnumerable<Dimension[]> PartsPerMaterial { get; init; }
}
