namespace ApplicationCore.Features.Orders.OrderExport.Handlers.ExtExport.Domain;

public record LevelDescriptor {
    public required int ParentId { get; set; }
    public required int LevelId { get; set; }
    public required string Name { get; set; }
    public required string Catalog { get; set; }
    public required string Materials { get; set; }
    public required string Fronts { get; set; }
    public required string Hardware { get; set; }
}
