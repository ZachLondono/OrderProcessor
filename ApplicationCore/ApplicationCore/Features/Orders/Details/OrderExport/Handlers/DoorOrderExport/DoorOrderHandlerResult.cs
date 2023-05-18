namespace ApplicationCore.Features.Orders.Details.OrderExport.Handlers.DoorOrderExport;

internal record DoorOrderHandlerResult(IEnumerable<string> GeneratedFiles, string? Error);
