using ApplicationCore.Features.CADCode.Contracts.Machining;

namespace ApplicationCore.Features.CADCode.Contracts;

public class CNCPart {

    public int Qty { get; init; }
    public double Width { get; init; }
    public double Length { get; init; }
    public string FileName { get; init; } = string.Empty; 
    public string Description { get; init; } = string.Empty;
    public PartMaterial Material { get; init; } = new();
    public IReadOnlyList<Token> Tokens { get; init; } = new List<Token>();

}
