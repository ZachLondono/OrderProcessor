namespace ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport.Contracts;

public record PPJob(string Name, DateTime OrderDate, string CustomerName, IEnumerable<PPProduct> Products);