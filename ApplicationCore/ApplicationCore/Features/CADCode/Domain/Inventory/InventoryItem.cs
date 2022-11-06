namespace ApplicationCore.Features.CADCode.Services.Domain.Inventory;

public class InventoryItem {

    public string Name { get; init; } = string.Empty;
    public double Thickness { get; init; }
    public bool IsGrained { get; init; }
    public IEnumerable<InventorySize> Sizes { get; init; } = new List<InventorySize>();

}
