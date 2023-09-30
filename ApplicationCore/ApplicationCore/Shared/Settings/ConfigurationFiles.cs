using Microsoft.Extensions.Configuration;

namespace ApplicationCore.Shared.Settings;

public class ConfigurationFiles {

    [ConfigurationKeyName("Tools")]
    public string ToolConfigFile { get; set; } = string.Empty;

}