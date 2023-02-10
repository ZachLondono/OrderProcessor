namespace ApplicationCore.Features.ProductPlanner.Domain;

public record VariableOverride {
    public required int LevelId { get; set; }
    public required PPUnits Units { get; set; }
    public required IDictionary<string, string> Parameters { get; set; }
}
