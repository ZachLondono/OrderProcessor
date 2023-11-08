using Microsoft.Extensions.Configuration;

namespace ApplicationCore.Shared.Settings;

public class HafeleDBOrderProviderSettings {

    [ConfigurationKeyName("vendor_id")]
    public Guid VendorId { get; set; }

    [ConfigurationKeyName("front_back_thickness_mm")]
    public double FrontBackThicknessMM { get; set; }

    [ConfigurationKeyName("side_thickness_mm")]
    public double SideThicknessMM { get; set; }

    [ConfigurationKeyName("bottom_thickness_mm")]
    public double BottomThicknessMM { get; set; }

    [ConfigurationKeyName("front_back_height_adj_mm")]
    public double FrontBackHeightAdjMM { get; set; }

}
