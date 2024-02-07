using Domain.Excel;
using Microsoft.Office.Interop.Excel;

namespace OrderLoading.LoadHafeleDBSpreadsheetOrderData;

public class ContactInformation {

    public required string Contact { get; set; }
    public required string Company { get; set; }
    public required string AccountNumber { get; set; }
    public required string Address1 { get; set; }
    public required string Address2 { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string Zip { get; set; }
    public required string Phone { get; set; }
    public required string Email { get; set; }

    public static ContactInformation ReadFromSheet(Worksheet sheet) {
        return new() {
            Contact = sheet.GetRangeValueOrDefault("V3", "UNKNOWN"),
            Company = sheet.GetRangeValueOrDefault("V4", "UNKNOWN"),
            AccountNumber = sheet.GetRangeValueOrDefault("V5", "UNKNOWN"),
            Address1 = sheet.GetRangeValueOrDefault("V6", "UNKNOWN"),
            Address2 = sheet.GetRangeValueOrDefault("V7", "UNKNOWN"),
            City = sheet.GetRangeValueOrDefault("V8", "UNKNOWN"),
            State = sheet.GetRangeValueOrDefault("V9", "UNKNOWN"),
            Zip = sheet.GetRangeValueOrDefault("V10", "UNKNOWN"),
            Phone = sheet.GetRangeValueOrDefault("V11", "UNKNOWN"),
            Email = sheet.GetRangeValueOrDefault("V12", "UNKNOWN"),
        };
    }

}
