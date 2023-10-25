namespace ApplicationCore.Shared.CNC.ReleasePDF.Styling;

public class Table {

    public string Title { get; init; } = string.Empty;
    public IReadOnlyList<IReadOnlyDictionary<string, string>> Content { get; init; } = new List<Dictionary<string, string>>();
    public IReadOnlyDictionary<string, float> ColumnWidths { get; init; } = new Dictionary<string, float>();

}
