using System.Text.Json.Serialization;

namespace ApplicationCore.Features.Tools.Domain;

internal class MachineToolMap {

    [JsonPropertyName("machine_name")]
    public string MachineName { get; set; } = string.Empty;

    [JsonPropertyName("tool_position_count")]
    public int ToolPositionCount { get; set; } = 0;

    [JsonPropertyName("tools")]
    public List<Tool> Tools { get; set; } = new();

}
