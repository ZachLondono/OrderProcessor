namespace ApplicationCore.Features.Orders.OrderExport.Handlers.DoorOrderExport;

public record DoorOrderExportResult(IEnumerable<string> GeneratedFiles, string? Error);
