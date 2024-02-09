using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;

namespace ApplicationCore.Shared.Settings.Tools;

public class MachineToolMap {

    [ConfigurationKeyName("machine_name")]
    [JsonPropertyName("machine_name")]
    public string MachineName { get; set; } = string.Empty;

    [ConfigurationKeyName("tool_position_count")]
    [JsonPropertyName("tool_position_count")]
    public int ToolPositionCount { get; set; } = 0;

    [ConfigurationKeyName("tools")]
    [JsonPropertyName("tools")]
    public List<Tool> Tools { get; set; } = new();

}
