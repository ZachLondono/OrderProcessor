using System.Text.Json.Serialization;

namespace ApplicationCore.Features.CNC.GCode.Domain.CADCode.Configuration;

public class OptimizerSettings
{

    [JsonPropertyName("iterations")]
    public int Iterations { get; init; }

    [JsonPropertyName("runTime")]
    public int RunTime { get; init; }

    [JsonPropertyName("utilization")]
    public int Utilization { get; init; }

    [JsonPropertyName("kerf")]
    public double Kerf { get; init; }

    [JsonPropertyName("panels")]
    public int Panels { get; init; }

    [JsonPropertyName("offset")]
    public int Offset { get; init; }

    [JsonPropertyName("showPattern")]
    public bool ShowPattern { get; init; }

    [JsonPropertyName("showResult")]
    public bool ShowResult { get; init; }

}