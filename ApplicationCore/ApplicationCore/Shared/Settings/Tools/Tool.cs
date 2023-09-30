using System.Text.Json.Serialization;

namespace ApplicationCore.Shared.Settings.Tools;

internal class Tool
{

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("alternative_names")]
    public List<string> AlternativeNames { get; set; } = new();

}
