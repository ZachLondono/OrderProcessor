using Microsoft.Extensions.Configuration;

namespace ApplicationCore.Shared.Settings;

public class ConfigurationFiles {

    [ConfigurationKeyName("Tools")]
    public string ToolConfigFile { get; set; } = string.Empty;

    [ConfigurationKeyName("Data")]
    public string DataConfigFile { get; set; } = string.Empty;

    [ConfigurationKeyName("Email")]
    public string EmailConfigFile { get; set; } = string.Empty;

    [ConfigurationKeyName("Paths")]
    public string PathsConfigFile { get; set; } = string.Empty;

}