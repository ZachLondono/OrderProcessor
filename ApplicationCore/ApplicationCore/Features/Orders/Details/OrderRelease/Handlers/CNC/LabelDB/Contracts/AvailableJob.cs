namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.LabelDB.Contracts;

public record AvailableJob(string Name, DateTime Created, string MachineName);
