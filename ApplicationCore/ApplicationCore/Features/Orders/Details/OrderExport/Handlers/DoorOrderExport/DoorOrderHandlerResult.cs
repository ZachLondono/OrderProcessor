namespace ApplicationCore.Features.Orders.Details.OrderExport.Handlers.DoorOrderExport;

internal record DoorOrderExportResult(IEnumerable<string> GeneratedFiles, string? Error);
