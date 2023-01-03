namespace ApplicationCore.Features.ProductPlanner.Domain;

public record JobDescriptor {
    public required int LevelId { get; set; }
    public required string Job { get; set; }
    public required DateTime Date { get; set; }
    public required string Catalog { get; set; }
    public required string Materials { get; set; }
    public required string Fronts { get; set; }
    public required string Hardware { get; set; }
}
