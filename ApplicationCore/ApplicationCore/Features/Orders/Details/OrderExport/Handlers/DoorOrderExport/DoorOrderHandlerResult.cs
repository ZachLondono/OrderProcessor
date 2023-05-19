namespace ApplicationCore.Features.Orders.Details.OrderExport.Handlers.DoorOrderExport;

public record DoorOrderExportResult(IEnumerable<string> GeneratedFiles, string? Error);
