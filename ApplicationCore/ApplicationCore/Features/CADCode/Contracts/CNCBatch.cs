namespace ApplicationCore.Features.CADCode.Contracts;

public class CNCBatch {

    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<CNCPart> Parts { get; init; } = new List<CNCPart>();

}