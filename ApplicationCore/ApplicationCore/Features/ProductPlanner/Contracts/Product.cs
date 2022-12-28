namespace ApplicationCore.Features.ProductPlanner.Contracts;

public record Product {
    public required int ParentId { get; init; }
    public required string Name { get; init; }
    public required int Qty { get; init; }
    public required PSIUnits Units { get; init; }
    public required int Pos { get; init; }
    public required string SeqText { get; init; }
    public required bool CustomSpec { get; init; }
    public required Dictionary<string, string> Parameters { get; set; }
}
