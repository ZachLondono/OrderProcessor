namespace ApplicationCore.Shared.CNC.WSXML.Report;

internal record Nest(string Id, string Name, IEnumerable<Part> Parts);
