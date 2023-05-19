namespace ApplicationCore.Features.Orders.Details.OrderExport.Handlers.DovetailOrderExport;

public record DovetailOrderExportResult(IEnumerable<string> GeneratedFiles, string? Error);