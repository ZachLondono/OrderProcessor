using Microsoft.Extensions.Configuration;

namespace ApplicationCore.Shared.Settings;

public class HafeleDBOrderProviderSettings {

    [ConfigurationKeyName("vendor_id")]
    public Guid VendorId { get; set; }

    [ConfigurationKeyName("material_thicknesses_mm")]
    public Dictionary<string, double> MaterialThicknessesMM { get; set; } = new();

    [ConfigurationKeyName("front_back_height_adj_mm")]
    public double FrontBackHeightAdjMM { get; set; }

}
