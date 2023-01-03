namespace ApplicationCore.Features.ProductPlanner.Contracts;

public record PPJob(JobDescriptor Job, IEnumerable<VariableOverride> VariableOverrides, IEnumerable<LevelDescriptor> Levels, IEnumerable<ProductRecord> Products);