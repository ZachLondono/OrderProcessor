namespace ApplicationCore.Features.CNC.ReleasePDF.WSXML;

internal record Nest(string Id, string Name, IEnumerable<Part> Parts);
