namespace Domain.ProductPlanner;

public record LevelVariableOverride {
    public required int LevelId { get; set; }
    public required PPUnits Units { get; set; }
    public required IDictionary<string, string> Parameters { get; set; }
}
