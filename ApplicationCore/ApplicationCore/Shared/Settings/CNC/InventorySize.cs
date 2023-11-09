using Microsoft.Extensions.Configuration;

namespace ApplicationCore.Shared.Settings.CNC;

public class InventorySize {

    [ConfigurationKeyName("width")]
    public double Width { get; set; }

    [ConfigurationKeyName("length")]
    public double Length { get; set; }

    [ConfigurationKeyName("priority")]
    public int Priority { get; set; } = 1;

}
