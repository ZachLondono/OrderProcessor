using System.Globalization;

namespace ApplicationCore.Features.CNC.GCode.Domain.CADCode;

public class OptimizationResult
{

    public IEnumerable<UnplacedPart> UnplacedParts { get; init; } = new List<UnplacedPart>();
    public IEnumerable<PlacedPart> PlacedParts { get; init; } = new List<PlacedPart>();
    public UsedInventory[] UsedInventory { get; init; } = Array.Empty<UsedInventory>();
    public string[] Programs { get; init; } = Array.Empty<string>();

}
