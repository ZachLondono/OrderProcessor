using Microsoft.Extensions.Configuration;

namespace ApplicationCore.Features.Shared.Settings;

public class ConfigurationFiles {

    [ConfigurationKeyName("Tools")]
    public string ToolConfigFile { get; set; } = string.Empty;

    [ConfigurationKeyName("Data")]
    public string DataConfigFile { get; set; } = string.Empty;

}