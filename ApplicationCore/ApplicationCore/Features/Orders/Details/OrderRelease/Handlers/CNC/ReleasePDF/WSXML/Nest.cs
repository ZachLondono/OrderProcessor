namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.WSXML;

internal record Nest(string Id, string Name, IEnumerable<Part> Parts);
