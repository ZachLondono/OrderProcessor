using System.Text.Json.Serialization;

namespace ApplicationCore.Features.DataFilePaths;

public class DataFilePathsWrapper {

    [JsonPropertyName("data")]
    public Shared.Data.DataFilePaths FilePaths { get; set; } = new();

    public DataFilePathsWrapper() { }

    public DataFilePathsWrapper(Shared.Data.DataFilePaths filePaths) {
        FilePaths = filePaths;
    }

}
