namespace ApplicationCore.Features.Orders.OrderExport.Handlers.DovetailOrderExport;

public record DovetailOrderExportResult(IEnumerable<string> GeneratedFiles, string? Error);