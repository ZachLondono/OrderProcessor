using System.Text.Json.Serialization;

namespace ApplicationCore.Features.DataFilePaths;

public class DataFilePathsWrapper {

    [JsonPropertyName("data")]
    public Shared.Settings.DataFilePaths FilePaths { get; set; } = new();

    public DataFilePathsWrapper() { }

    public DataFilePathsWrapper(Shared.Settings.DataFilePaths filePaths) {
        FilePaths = filePaths;
    }

}
