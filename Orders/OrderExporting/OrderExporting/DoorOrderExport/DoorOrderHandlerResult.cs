namespace OrderExporting.DoorOrderExport;

public record DoorOrderExportResult(IEnumerable<string> GeneratedFiles, string? Error);
