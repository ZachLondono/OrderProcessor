using Microsoft.Office.Interop.Excel;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadDoweledDBSpreadsheetOrderData.Models;

public record Header {

    public DateTime OrderDate { get; init; }
    public DateTime DueDate { get; init; }
    public string VendorName { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string OrderNumber { get; init; } = string.Empty;
    public string OrderName { get; init; } = string.Empty;
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

        return new() {
            OrderDate = DateTime.Today, // worksheet.Range["OrderDate"].Value2,
            DueDate = DateTime.Today, // worksheet.Range["DueDate"].Value2,
            VendorName = worksheet.Range["Vendor"].Value2.ToString(),
            CustomerName = worksheet.Range["CustomerName"].Value2.ToString(),
            OrderNumber = worksheet.Range["JobNumber"].Value2.ToString(),
            OrderName = worksheet.Range["JobName"].Value2.ToString(),
            ConnectorType = worksheet.Range["SelectedConnectionType"].Value2.ToString(),
            Construction = worksheet.Range["SelectedConstructionOption"].Value2.ToString(),
            Units = worksheet.Range["SelectedUnits"].Value2.ToString(),
            SpecialInstructions = string.Join("; ", noteSegments.Where(s => !string.IsNullOrWhiteSpace(s)))
        };

    }

}


