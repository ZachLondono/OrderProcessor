using System.Globalization;

namespace ApplicationCore.Features.CADCode.Services.Domain.CADCode;

internal class OptimizationResult {

    public IEnumerable<UnplacedPart> UnplacedParts { get; init; } = new List<UnplacedPart>();
    public IEnumerable<PlacedPart> PlacedParts { get; init; } = new List<PlacedPart>();
    public UsedInventory[] UsedInventory { get; init; } = Array.Empty<UsedInventory>();
    public string[] Programs { get; init; } = Array.Empty<string>();

}
