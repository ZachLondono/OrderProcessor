namespace ApplicationCore.Features.ProductPlanner.Contracts;

public record PSIJob(JobDescriptor Job, IEnumerable<VariableOverride> VariableOverrides, IEnumerable<LevelDescriptor> Levels, IEnumerable<Product> Products);