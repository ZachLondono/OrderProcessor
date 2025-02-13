using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;

namespace OrderExporting.CNC.Settings;

public class InventoryItem {

    [ConfigurationKeyName("thickness")]
    [JsonPropertyName("thickness")]
    public double Thickness { get; set; }

    [ConfigurationKeyName("sizes")]
    [JsonPropertyName("sizes")]
    public InventorySize[] Sizes { get; set; } = Array.Empty<InventorySize>();

}
