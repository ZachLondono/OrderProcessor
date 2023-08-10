using Microsoft.Office.Interop.Excel;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadDoweledDBSpreadsheetOrderData.Models;

public record Header {

    public DateTime OrderDate { get; init; }
    public DateTime DueDate { get; init; }
    public string VendorName { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string OrderNumber { get; init; } = string.Empty;
    public string OrderName { get; init; } = string.Empty;
    public string ShippingMethod { get; init; } = string.Empty;
    public string ConnectorType { get; init; } = string.Empty;
    public string Construction { get; init; } = string.Empty;
    public string Units { get; init; } = string.Empty;
    public string SpecialInstructions { get; init; } = string.Empty;

    public static Header ReadFromSheet(Worksheet worksheet) {

        string[] noteSegments = new string[] {
                worksheet.GetRangeStringValue("SpecialInstructions"),
                worksheet.GetRangeStringValue("SpecialInstructions_2"),
                worksheet.GetRangeStringValue("SpecialInstructions_3"),
                worksheet.GetRangeStringValue("SpecialInstructions_4"),
                worksheet.GetRangeStringValue("SpecialInstructions_5"),
                worksheet.GetRangeStringValue("SpecialInstructions_6"),
                worksheet.GetRangeStringValue("SpecialInstructions_7"),
                worksheet.GetRangeStringValue("SpecialInstructions_8"),
                worksheet.GetRangeStringValue("SpecialInstructions_9"),
                worksheet.GetRangeStringValue("SpecialInstructions_10"),
                worksheet.GetRangeStringValue("SpecialInstructions_11"),
                worksheet.GetRangeStringValue("SpecialInstructions_12"),
            };

        string orderDateStr = worksheet.GetRangeStringValue("OrderDate");
        if (!DateTime.TryParse(orderDateStr, out DateTime orderDate)) {
            orderDate = DateTime.Today;
        }
        string dueDateStr = worksheet.GetRangeStringValue("DueDate");
        if (!DateTime.TryParse(dueDateStr, out DateTime dueDate)) {
            dueDate = DateTime.Today;
        }

        return new() {
            OrderDate = orderDate,
            DueDate = dueDate,
            VendorName = worksheet.GetRangeStringValue("Vendor"),
            CustomerName = worksheet.GetRangeStringValue("CustomerName"),
            OrderNumber = worksheet.GetRangeStringValue("JobNumber"),
            OrderName = worksheet.GetRangeStringValue("JobName"),
            ShippingMethod = worksheet.GetRangeStringValue("ShippingMethod"),
            ConnectorType = worksheet.GetRangeStringValue("SelectedConnectionType"),
            Construction = worksheet.GetRangeStringValue("SelectedConstructionOption"),
            Units = worksheet.GetRangeStringValue("SelectedUnits"),
            SpecialInstructions = string.Join("; ", noteSegments.Where(s => !string.IsNullOrWhiteSpace(s)))
        };

    }

}


