namespace ApplicationCore.Shared.CNC.WorkOrderReleaseEmail;

public record Job(string JobName, IEnumerable<UsedMaterial> UsedMaterials);
