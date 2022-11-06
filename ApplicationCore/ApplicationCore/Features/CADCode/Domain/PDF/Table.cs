namespace ApplicationCore.Features.CADCode.Services.Domain.PDF;

internal class Table {

    public string Title { get; init; } = string.Empty;
    public IReadOnlyList<IReadOnlyDictionary<string, string>> Content { get; init; } = new List<Dictionary<string, string>>();

}
