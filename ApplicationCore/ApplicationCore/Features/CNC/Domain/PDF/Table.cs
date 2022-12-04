namespace ApplicationCore.Features.CNC.Services.Domain.PDF;

public class Table {

    public string Title { get; init; } = string.Empty;
    public IReadOnlyList<IReadOnlyDictionary<string, string>> Content { get; init; } = new List<Dictionary<string, string>>();

}
