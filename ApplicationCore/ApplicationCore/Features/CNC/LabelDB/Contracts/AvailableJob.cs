namespace ApplicationCore.Features.CNC.LabelDB.Contracts;

public record AvailableJob(string Name, DateTime Created, string MachineName);
