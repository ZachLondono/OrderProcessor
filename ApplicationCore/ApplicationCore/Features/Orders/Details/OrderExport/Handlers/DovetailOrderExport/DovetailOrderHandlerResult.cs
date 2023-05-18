namespace ApplicationCore.Features.Orders.Details.OrderExport.Handlers.DovetailOrderExport;

internal record DovetailOrderHandlerResult(IEnumerable<string> GeneratedFiles, string? Error);