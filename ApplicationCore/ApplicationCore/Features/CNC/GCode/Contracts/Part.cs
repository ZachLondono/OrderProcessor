using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.CNC.GCode.Contracts;

public class Part {
    public required int Qty { get; init; }
    public required Dimension Length { get; init; }
    public required Dimension Width { get; init; }
    public required string Description { get; init; }
    public required bool ContainsShape { get; init; }
    public required PartFace PrimaryFace { get; init; }
    public required PartFace? SecondaryFace { get; init; }
    public required IEnumerable<LabelField> LabelFields { get; init; }
}
