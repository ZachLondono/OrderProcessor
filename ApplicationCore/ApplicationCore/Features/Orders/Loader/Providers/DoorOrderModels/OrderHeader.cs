using Microsoft.Office.Interop.Excel;

namespace ApplicationCore.Features.Orders.Loader.Providers.DoorOrderModels;

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
            VendorName = sheet.Range["Vendor"].Value2,
            CompanyName = sheet.Range["Company"].Value2,
            Phone = sheet.Range["CustomerPhone"].Value2,
            InvoiceFirstName = sheet.Range["CustomerInvoiceFirstName"].Value2,
            InvoiceEmail = sheet.Range["CustomerInvoiceEmail"].Value2,
            ConfirmationFirstName = sheet.Range["CustomerConfirmationFirstName"].Value2,
            ConfirmationEmail = sheet.Range["CustomerConfirmationEmail"].Value2,
            Address1 = sheet.Range["CustomerAddress1"].Value2,
            Address2 = sheet.Range["CustomerAddress2"].Value2,
            Units = sheet.Range["units"].Value2,
            TrackingNumber = sheet.Range["JobNumber"].Value2,
            JobName = sheet.Range["JobName"].Value2,
            OrderDate = DateTime.FromOADate(sheet.Range["OrderDate"].Value2),
            Freight = (decimal) sheet.Range["Freight"].Value2,
            PanelDrop = sheet.Range["PanelDrop"].Value2,
            Finish = sheet.Range["FinishOption"].Value2,
            Color = sheet.Range["FinishColor"].Value2,
            Style = sheet.Range["FramingBead"].Value2,
            EdgeProfile = sheet.Range["EdgeDetail"].Value2,
            PanelDetail = sheet.Range["PanelDetail"].Value2
        };

    public enum UnitType {
        Inches,
        Millimeters
    }

}
