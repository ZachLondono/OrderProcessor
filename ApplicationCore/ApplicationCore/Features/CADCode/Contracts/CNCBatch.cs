namespace ApplicationCore.Features.CADCode.Contracts;

internal class CNCBatch {

    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<CNCPart> Parts { get; init; } = new List<CNCPart>();

}