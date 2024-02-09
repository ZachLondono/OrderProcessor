using Microsoft.Extensions.Configuration;

namespace OrderExporting.CNC.Settings;

public class CNCSettings {

    /// <summary>
    /// A map of machine names to settings used for gcode generation
    /// </summary>
    [ConfigurationKeyName("machine_settings")]
    public Dictionary<string, MachineSettings> MachineSettings { get; set; } = new();

    /// <summary>
    /// This is the size that is used whenever a material name and thickness is not available in the Inventory dictionary
    /// </summary>
    [ConfigurationKeyName("default_inventory_size")]
    public InventorySize DefaultInventorySize { get; set; } = new();

    /// <summary>
    /// Maps a material name to an inventory item. That inventory item will be used when generating GCode for a part with that name (as long as the thickness is also the same).
    /// </summary>
    [ConfigurationKeyName("inventory")]
    public Dictionary<string, InventoryItem> Inventory { get; set; } = new();

}