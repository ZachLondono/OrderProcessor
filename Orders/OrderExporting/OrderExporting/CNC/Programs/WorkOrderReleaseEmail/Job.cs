namespace OrderExporting.CNC.Programs.WorkOrderReleaseEmail;

public record Job(string JobName, IEnumerable<UsedMaterial> UsedMaterials);
