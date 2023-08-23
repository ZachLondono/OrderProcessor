using ApplicationCore.Features.Orders.Shared.Domain.Excel;
using Microsoft.Office.Interop.Excel;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetOrderSpreadsheetOrderData.Models;

internal class Cover {

	public string CustomerName { get; set; } = string.Empty;
	public string JobName { get; set; } = string.Empty;
	public DateTime OrderDate { get; set; }
	public DateTime DueDate { get; set; }
	public string MaterialColor { get; set; } = string.Empty;
	public string ShippingInformation { get; set; } = string.Empty;
	public string SpecialRequirements { get; set; } = string.Empty;
	public IEnumerable<Molding> Moldings { get; set; } = Enumerable.Empty<Molding>();

	public static Cover ReadFromWorksheet(Worksheet worksheet) {

		return new() {
			CustomerName = worksheet.GetRangeValueOrDefault("CustomerName", ""),
			JobName = worksheet.GetRangeValueOrDefault("JobName", ""),
			OrderDate = DateTime.FromOADate(worksheet.GetRangeValueOrDefault<double?>("E4", null) ?? DateTime.Now.ToOADate()),
			DueDate = DateTime.FromOADate(worksheet.GetRangeValueOrDefault<double?>("E5", null) ?? DateTime.Now.ToOADate()),
			MaterialColor = worksheet.GetRangeValueOrDefault("E8", ""),
			ShippingInformation = worksheet.GetRangeValueOrDefault("E25", ""),
			SpecialRequirements = worksheet.GetRangeValueOrDefault("A30", ""),
			Moldings = new Molding[] {
				Molding.ReadFromWorksheet(worksheet, 46),
				Molding.ReadFromWorksheet(worksheet, 47),
				Molding.ReadFromWorksheet(worksheet, 48),
			}
		};

	}

}

