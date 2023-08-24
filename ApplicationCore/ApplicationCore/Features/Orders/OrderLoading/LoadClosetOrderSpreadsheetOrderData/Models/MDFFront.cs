using ApplicationCore.Features.Orders.Shared.Domain.Excel;
using Microsoft.Office.Interop.Excel;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetOrderSpreadsheetOrderData.Models;

internal record MDFFront : IWorksheetReadable<MDFFront> {

	public int LineNumber { get; set; }
	public int Qty { get; set; }
	public double Height { get; set; }
	public double Width { get; set; }
	public string AStyle { get; set; } = string.Empty;
	public string Note { get; set; } = string.Empty;
	public decimal UnitPrice { get; set; }
	public decimal TotalPrice { get; set; }

	public static int FirstRow => 6;

	public static int RowStep => 1; 

	public static MDFFront ReadFromWorksheet(Worksheet worksheet, int row) {
		int lineNum = (int) worksheet.GetRangeValueOrDefault($"A{row}", 0.0);
		int qty = (int) worksheet.GetRangeValueOrDefault($"B{row}", 0.0);
		var height = worksheet.GetRangeValueOrDefault($"E{row}", 0.0);
		var width = worksheet.GetRangeValueOrDefault($"F{row}", 0.0);
		var aStyle = worksheet.GetRangeValueOrDefault($"G{row}", "");
		var note = worksheet.GetRangeValueOrDefault($"G{row}", "");
		var unitPrice = (decimal) worksheet.GetRangeValueOrDefault($"G{row}", 0.0);
		var totalPrice = (decimal) worksheet.GetRangeValueOrDefault($"G{row}", 0.0);
		return new() {
			LineNumber = lineNum,
			Qty = qty,
			Height = height,
			Width = width,
			AStyle = aStyle,
			Note = note,
			UnitPrice = unitPrice,
			TotalPrice = totalPrice
		};
	}

}

