namespace ApplicationCore.Shared.CNC.WorkOrderReleaseEmail;

public record ReleasedWorkOrderSummary(IEnumerable<Job> ReleasedJobs, string? Note);
