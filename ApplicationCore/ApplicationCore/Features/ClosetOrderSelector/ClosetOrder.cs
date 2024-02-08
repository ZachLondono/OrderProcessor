namespace ApplicationCore.Features.ClosetOrderSelector;

public record ClosetOrder(string Customer, string OrderName, string OrderNumber, DateTime OrderDate, DateTime DueDate, string ReportFilePath, string OrderFile, string OrderFileDirectory);
