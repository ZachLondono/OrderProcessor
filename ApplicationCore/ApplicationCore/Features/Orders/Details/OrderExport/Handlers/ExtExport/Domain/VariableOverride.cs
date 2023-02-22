namespace ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport.Domain;

public record VariableOverride {
    public required int LevelId { get; set; }
    public required PPUnits Units { get; set; }
    public required IDictionary<string, string> Parameters { get; set; }
}
