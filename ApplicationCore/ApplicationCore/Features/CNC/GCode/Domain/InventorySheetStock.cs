using ApplicationCore.Features.CNC.GCode.Domain.Inventory;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.CNC.GCode.Domain;

public record InventorySheetStock {
    public required string Name { get; init; }
    public required Dimension Thickness { get; init; }
    public required bool IsGrained { get; init; }
    public required IEnumerable<InventorySize> Sizes { get; init; }
}
