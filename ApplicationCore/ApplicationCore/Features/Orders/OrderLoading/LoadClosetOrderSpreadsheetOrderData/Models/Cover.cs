using Domain.Orders.Excel;
using Microsoft.Office.Interop.Excel;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetOrderSpreadsheetOrderData.Models;

internal class Cover {

    public string CustomerName { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime DueDate { get; set; }
    public string MaterialColor { get; set; } = string.Empty;
    public string ShippingInformation { get; set; } = string.Empty;
    public string SpecialRequirements { get; set; } = string.Empty;
    public decimal InstallCamsCharge { get; set; }
    public decimal ManualCharge { get; set; }
    public decimal RushCharge { get; set; }
    public decimal DeliveryCharge { get; set; }
    public decimal Tax { get; set; }
    public IEnumerable<Molding> Moldings { get; set; } = Enumerable.Empty<Molding>();

    public static Cover ReadFromWorksheet(Worksheet worksheet) {

        return new() {
            CustomerName = worksheet.GetRangeValueOrDefault("CustomerName", ""),
            AddressLine1 = worksheet.GetRangeValueOrDefault("A4", ""),
            AddressLine2 = worksheet.GetRangeValueOrDefault("A5", ""),
            CustomerPhone = worksheet.GetRangeValueOrDefault("A6", ""),
            CustomerEmail = worksheet.GetRangeValueOrDefault("A7", ""),
            JobName = worksheet.GetRangeValueOrDefault("JobName", ""),
            OrderDate = DateTime.FromOADate(worksheet.GetRangeValueOrDefault<double?>("E4", null) ?? DateTime.Now.ToOADate()),
            DueDate = DateTime.FromOADate(worksheet.GetRangeValueOrDefault<double?>("E5", null) ?? DateTime.Now.ToOADate()),
            MaterialColor = worksheet.GetRangeValueOrDefault("E8", ""),
            ShippingInformation = worksheet.GetRangeValueOrDefault("E25", ""),
            SpecialRequirements = worksheet.GetRangeValueOrDefault("A30", ""),
            InstallCamsCharge = (decimal)worksheet.GetRangeValueOrDefault("C15", 0.0),
            ManualCharge = (decimal)worksheet.GetRangeValueOrDefault("C17", 0.0),
            RushCharge = (decimal)worksheet.GetRangeValueOrDefault("C18", 0.0),
            DeliveryCharge = (decimal)worksheet.GetRangeValueOrDefault("C20", 0.0),
            Tax = (decimal)worksheet.GetRangeValueOrDefault("C21", 0.0),
            Moldings = new Molding[] {
                Molding.ReadFromWorksheet(worksheet, 46),
                Molding.ReadFromWorksheet(worksheet, 47),
                Molding.ReadFromWorksheet(worksheet, 48),
            }
        };

    }

}

