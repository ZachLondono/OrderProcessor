using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;

namespace ApplicationCore.Shared.Settings.Tools;

internal class ToolConfiguration {

    [ConfigurationKeyName("machine_tool_maps")]
    [JsonPropertyName("machine_tool_maps")]
    public List<MachineToolMap> MachineToolMaps { get; set; } = new();

}
