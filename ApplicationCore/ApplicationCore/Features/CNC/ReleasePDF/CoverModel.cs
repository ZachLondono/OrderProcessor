namespace ApplicationCore.Features.CNC.ReleasePDF;

public class CoverModel
{

    public string Title { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> Info { get; init; } = new Dictionary<string, string>();
    public IReadOnlyList<Table> Tables { get; init; } = new List<Table>();

}
