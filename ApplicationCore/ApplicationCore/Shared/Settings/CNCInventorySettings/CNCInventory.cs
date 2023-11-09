using Microsoft.Extensions.Configuration;

namespace ApplicationCore.Shared.Settings.CNCInventorySettings;

public class CNCInventory {

    [ConfigurationKeyName("default_size")]
    public InventorySize DefaultSize { get; set; } = new();

    [ConfigurationKeyName("inventory")]
    public Dictionary<string, InventoryItem> Inventory { get; set; } = new();

}