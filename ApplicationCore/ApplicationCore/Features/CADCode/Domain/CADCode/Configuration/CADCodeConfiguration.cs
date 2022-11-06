using System.Text.Json.Serialization;

namespace ApplicationCore.Features.CADCode.Services.Domain.CADCode.Configuration;

public class CADCodeConfiguration {

    [JsonPropertyName("startingProgramNumber")]
    public int StartingProgramNumber { get; init; }
    
    [JsonPropertyName("startingSubProgramNumber")]
    public int StartingSubProgramNumber { get; init; }
    
    [JsonPropertyName("labelDataOutputDirectory")]
    public string LabelDataOutputDirectory { get; init; } = string.Empty;
    
    [JsonPropertyName("pictureFileOutputDirectory")]
    public string PictureFileOutputDirectory { get; init; } = string.Empty;
    
    [JsonPropertyName("labelDesignFile")]
    public string LabelDesignFile { get; init; } = string.Empty;
    
    [JsonPropertyName("optimizerSettings")]
    public OptimizerSettings OptimizerSettings { get; init; } = new();

}
