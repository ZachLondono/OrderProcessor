namespace ApplicationCore.Features.ProductPlanner.Domain;

public record VariableOverride {
    public required int LevelId { get; set; }
    public required PSIUnits Units { get; set; }
    public required Dictionary<string, string> Parameters { get; set; }
}
