using ApplicationCore.Shared.CNC.ReleasePDF.Styling;

namespace ApplicationCore.Shared.CNC.ReleasePDF.PDFModels;

public class CoverModel {

    public string Title { get; init; } = string.Empty;
    public DateTime TimeStamp { get; init; } = DateTime.Now;
    public string ApplicationVersion { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> Info { get; init; } = new Dictionary<string, string>();
    public IReadOnlyList<Table> Tables { get; init; } = new List<Table>();

}
