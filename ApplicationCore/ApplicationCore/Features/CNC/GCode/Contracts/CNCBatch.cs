namespace ApplicationCore.Features.CNC.GCode.Contracts;

public class CNCBatch {

    public required string Name { get; init; }
    public required IReadOnlyList<CNCPart> Parts { get; init; }

}