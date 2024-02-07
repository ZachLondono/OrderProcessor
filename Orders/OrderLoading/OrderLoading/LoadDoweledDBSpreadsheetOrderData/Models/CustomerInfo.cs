using Microsoft.Office.Interop.Excel;

namespace OrderLoading.LoadDoweledDBSpreadsheetOrderData.Models;

public record CustomerInfo {

    public string Contact { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Line1 { get; init; } = string.Empty;
    public string Line2 { get; init; } = string.Empty;
    public string Line3 { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string Zip { get; init; } = string.Empty;

    public static CustomerInfo ReadFromSheet(Worksheet worksheet) {

        return new() {
            Contact = worksheet.GetRangeStringValue("CustomerInfo_Contact"),
            Email = worksheet.GetRangeStringValue("CustomerInfo_Email"),
            Line1 = worksheet.GetRangeStringValue("CustomerInfo_Line1"),
            Line2 = worksheet.GetRangeStringValue("CustomerInfo_Line2"),
            Line3 = worksheet.GetRangeStringValue("CustomerInfo_Line3"),
            City = worksheet.GetRangeStringValue("CustomerInfo_City"),
            State = worksheet.GetRangeStringValue("CustomerInfo_State"),
            Zip = worksheet.GetRangeStringValue("CustomerInfo_Zip")
        };

    }

}


