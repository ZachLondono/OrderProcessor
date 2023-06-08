using ApplicationCore.Features.Orders.Shared.Domain.Excel;
using Microsoft.Office.Interop.Excel;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadDoorSpreadsheetOrderData.DoorOrderModels;

public class OrderHeader {

    public required string VendorName { get; set; }
    public required string CompanyName { get; set; }
    public required string Phone { get; set; }
    public required string InvoiceFirstName { get; set; }
    public required string InvoiceEmail { get; set; }
    public required string ConfirmationFirstName { get; set; }
    public required string ConfirmationEmail { get; set; }
    public required string Address1 { get; set; }
    public required string Address2 { get; set; }
    public required string Units { get; set; }
    public required string TrackingNumber { get; set; }
    public required string JobName { get; set; }
    public required DateTime OrderDate { get; set; }
    public required decimal Freight { get; set; }
    public required double PanelDrop { get; set; }
    public required string Finish { get; set; }
    public required string Color { get; set; }
    public required string Style { get; set; }
    public required string EdgeProfile { get; set; }
    public required string PanelDetail { get; set; }

    public UnitType GetUnitType() {

        if (Units.Contains("English")) return UnitType.Inches;
        else if (Units.Contains("Metric")) return UnitType.Millimeters;

        throw new InvalidOperationException($"Unknown unit type '{Units}'");

    }

    public static OrderHeader ReadFromWorksheet(Worksheet sheet)
        => new() {
            VendorName = sheet.GetRangeValueOrDefault("Vendor", ""),
            CompanyName = sheet.GetRangeValueOrDefault("Company", ""),
            Phone = sheet.GetRangeValueOrDefault("CustomerPhone", ""),
            InvoiceFirstName = sheet.GetRangeValueOrDefault("CustomerInvoiceFirstName", ""),
            InvoiceEmail = sheet.GetRangeValueOrDefault("CustomerInvoiceEmail", ""),
            ConfirmationFirstName = sheet.GetRangeValueOrDefault("CustomerConfirmationFirstName", ""),
            ConfirmationEmail = sheet.GetRangeValueOrDefault("CustomerConfirmationEmail", ""),
            Address1 = sheet.GetRangeValueOrDefault("CustomerAddress1", ""),
            Address2 = sheet.GetRangeValueOrDefault("CustomerAddress2", ""),
            Units = sheet.GetRangeValueOrDefault("units", ""),
            TrackingNumber = sheet.GetRangeValueOrDefault("JobNumber", ""),
            JobName = sheet.GetRangeValueOrDefault("JobName", ""),
            Freight = sheet.GetRangeValueOrDefault("Freight", 0M),
            PanelDrop = sheet.GetRangeValueOrDefault<double>("PanelDrop", 0),
            Finish = sheet.GetRangeValueOrDefault("FinishOption", ""),
            Color = sheet.GetRangeValueOrDefault("FinishColor", ""),
            Style = sheet.GetRangeValueOrDefault("FramingBead", ""),
            EdgeProfile = sheet.GetRangeValueOrDefault("EdgeDetail", ""),
            PanelDetail = sheet.GetRangeValueOrDefault("PanelDetail", ""),
            OrderDate = DateTime.FromOADate(sheet.GetRangeValueOrDefault<double?>("OrderDate", null) ?? DateTime.Now.ToOADate()),
        };

    public enum UnitType {
        Inches,
        Millimeters
    }

}
