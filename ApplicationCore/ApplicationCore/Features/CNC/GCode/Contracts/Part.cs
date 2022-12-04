using ApplicationCore.Features.CNC.GCode.Contracts.Machining;

namespace ApplicationCore.Features.CNC.GCode.Contracts;

public class CNCPart
{

    public required int Qty { get; init; }
    public required double Length { get; init; }
    public required double Width { get; init; }
    public required string FileName { get; init; }
    public required string Description { get; init; }
    public required PartMaterial Material { get; init; }
    public required IReadOnlyList<Token> Tokens { get; init; }
    public required bool ContainsShape { get; init; }

}
