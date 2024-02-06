namespace Domain.ProductPlanner;

public record ProductRecord
{
    public required int ParentId { get; init; }
    public required string Name { get; init; }
    public required int Qty { get; init; }
    public required PPUnits Units { get; init; }
    public required int Pos { get; init; }
    public required string SeqText { get; init; }
    public required bool CustomSpec { get; init; }
    public required string Comment { get; init; }
    public required Guid ProductId { get; init; }
    public required IDictionary<string, string> Parameters { get; set; }
}
