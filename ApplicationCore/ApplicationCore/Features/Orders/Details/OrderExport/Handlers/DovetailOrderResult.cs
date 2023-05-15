namespace ApplicationCore.Features.Orders.Details.OrderExport.Handlers;

internal record DovetailOrderHandlerResult(IEnumerable<string> GeneratedFiles, string? Error);