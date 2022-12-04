namespace ApplicationCore.Features.CNC.Contracts;

public record AvailableJob(string Name, DateTime Created, string MachineName);
