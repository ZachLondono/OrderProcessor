namespace ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport.Contracts;

public record PPJob(string Name, DateTime OrderDate, IEnumerable<PPProduct> Products);