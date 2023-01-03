namespace ApplicationCore.Features.ProductPlanner.Contracts;

public record PPJob(string Name, DateTime OrderDate, IEnumerable<PPProduct> Products);