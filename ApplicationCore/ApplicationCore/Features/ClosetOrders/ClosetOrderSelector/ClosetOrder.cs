namespace ApplicationCore.Features.ClosetOrders.ClosetOrderSelector;

public record ClosetOrder(
    string Customer,
    string OrderName,
    string OrderNumber,
    DateTime OrderDate,
    DateTime DueDate,
    bool ContainsMDFFronts,
    bool ContainsDovetailBoxes,
    bool ContainsOther,
    string ReportFilePath,
    string OrderFile,
    string OrderFileDirectory,
    bool IsSandBoxed);
