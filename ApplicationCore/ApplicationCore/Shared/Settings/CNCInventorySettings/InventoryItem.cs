using Microsoft.Extensions.Configuration;

namespace ApplicationCore.Shared.Settings.CNCInventorySettings;

public class InventoryItem {

    [ConfigurationKeyName("thickness")]
    public double Thickness { get; set; }

    [ConfigurationKeyName("sizes")]
    public InventorySize[] Sizes { get; set; } = Array.Empty<InventorySize>();

}
