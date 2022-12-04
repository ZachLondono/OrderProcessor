using System.Text.Json.Serialization;

namespace ApplicationCore.Features.CNC.GCode.Domain.CADCode.Configuration;

public class CADCodeMachineConfiguration
{


    [JsonPropertyName("machineName")]
    public string MachineName { get; init; } = string.Empty;

    [JsonPropertyName("toolFilePath")]
    public string ToolFilePath { get; init; } = string.Empty;

    [JsonPropertyName("nestedGCodeOutputDirectory")]
    public string NestedGCodeOutputDirectory { get; init; } = string.Empty;

    [JsonPropertyName("singleGCodeOutputDirectory")]
    public string SingleGCodeOutputDirectory { get; init; } = string.Empty;

}
