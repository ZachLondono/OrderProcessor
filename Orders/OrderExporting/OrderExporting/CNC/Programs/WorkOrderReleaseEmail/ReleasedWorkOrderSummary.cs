namespace OrderExporting.CNC.Programs.WorkOrderReleaseEmail;

public record ReleasedWorkOrderSummary(IEnumerable<Job> ReleasedJobs, bool ContainsDrawerBoxes, bool ContainsMDFDoors, bool ContainsFivePieceDoors, string? Note);
