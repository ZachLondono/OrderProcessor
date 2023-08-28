namespace ApplicationCore.Features.OpenDoorOrders;

public record DoorOrder(string Customer, string Vendor, string OrderName, string OrderNumber, string ReportFilePath, string OrderFileDirectory);
