namespace ApplicationCore.Features.ClosetOrderSelector;

public record ClosetOrder(
    string Customer,
    string OrderName,
    string OrderNumber,
    DateTime OrderDate,
    DateTime DueDate,
    bool ContainsMDFFronts,
    bool ContainsDovetailBoxes,
    string ReportFilePath,
    string OrderFile,
    string OrderFileDirectory);
