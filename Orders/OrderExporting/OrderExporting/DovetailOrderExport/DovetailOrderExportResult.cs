namespace OrderExporting.DovetailOrderExport;

public record DovetailOrderExportResult(IEnumerable<string> GeneratedFiles, string? Error);