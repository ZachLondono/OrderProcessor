using ApplicationCore.Features.CNC.GCode.Domain.CADCode;

namespace ApplicationCore.Features.CNC.GCode.Contracts.Results;

public record OptimizationResult {
    public IEnumerable<UnplacedPart> UnplacedParts { get; init; } = new List<UnplacedPart>();
    public IEnumerable<PlacedPart> PlacedParts { get; init; } = new List<PlacedPart>();
    public UsedInventory[] UsedInventory { get; init; } = Array.Empty<UsedInventory>();
    public string[] Programs { get; init; } = Array.Empty<string>();
}
