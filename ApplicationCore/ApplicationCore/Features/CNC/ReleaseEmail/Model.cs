namespace ApplicationCore.Features.CNC.ReleaseEmail;

public record Model(IEnumerable<Job> ReleasedJobs, string? Note);

public record Job(string JobName, IEnumerable<UsedMaterial> UsedMaterials);

public record UsedMaterial(int Qty, string Name, double Width, double Length, double Thickness);
