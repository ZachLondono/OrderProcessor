using ApplicationCore.Features.Orders.Shared.Domain.Excel;
using Microsoft.Office.Interop.Excel;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadHafeleDBSpreadsheetOrderData;

public class OrderDetails {

    public required DateTime OrderDate { get; set; }
    public required string Company { get; set; }
    public required string LabelName { get; set; }
    public required string OrderContact { get; set; }
    public required string AccountNumber { get; set; }
    public required string PurchaseOrder { get; set; }
    public required string JobName { get; set; }
    public required string ProductionTime { get; set; }
    public required string HafelePO { get; set; }
    public required string HafeleOrderNumber { get; set; }
    public required string UnitFormat { get; set; }

    public static OrderDetails ReadFromSheet(Worksheet sheet) {

        double orderDate = sheet.GetRangeValueOrDefault("OrderDate", 0.0);

        return new() {
            OrderDate = DateTime.FromOADate(orderDate),
            Company = sheet.GetRangeValueOrDefault("Company", "UNKNOWN"),
            LabelName = sheet.GetRangeValueOrDefault("LabelName", "UNKNOWN"),
            OrderContact = sheet.GetRangeValueOrDefault("OrderContact", "UNKNOWN"),
            AccountNumber = sheet.GetRangeValueOrDefault("AccountNumber", "UNKNOWN"),
            PurchaseOrder = sheet.GetRangeValueOrDefault("PurchaseOrder", "UNKNOWN"),
            JobName = sheet.GetRangeValueOrDefault("Jobname", "UNKNOWN"),
            ProductionTime = sheet.GetRangeValueOrDefault("ProductionSelection", "UNKNOWN"),
            HafelePO = sheet.GetRangeValueOrDefault("HafelePO", "UNKNOWN"),
            HafeleOrderNumber = sheet.GetRangeValueOrDefault("HafeleOrderNumber", "UNKNOWN"),
            UnitFormat = sheet.GetRangeValueOrDefault("Notation", "UNKNOWN")
        };

    }

}
