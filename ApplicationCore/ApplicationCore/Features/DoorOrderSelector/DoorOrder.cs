namespace ApplicationCore.Features.OpenDoorOrders;

public record DoorOrder(string Customer,
                        string Vendor,
                        string OrderName,
                        string OrderNumber,
                        string ReportFilePath,
                        string OrderFileDirectory,
                        string OrderFile,
                        DateTime? ReleasedDate,
                        DateTime? OrderDate,
                        DateTime? DueDate,
                        int ItemCount,
                        bool IsSandBoxed);
