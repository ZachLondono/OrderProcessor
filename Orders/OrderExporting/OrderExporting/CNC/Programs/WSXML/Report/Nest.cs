namespace OrderExporting.CNC.Programs.WSXML.Report;

public record Nest(string Id, string Name, IEnumerable<Part> Parts);
