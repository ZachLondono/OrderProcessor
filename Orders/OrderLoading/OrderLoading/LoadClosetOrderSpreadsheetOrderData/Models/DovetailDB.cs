using Domain.Excel;
using Microsoft.Office.Interop.Excel;

namespace OrderLoading.LoadClosetOrderSpreadsheetOrderData.Models;

public record DovetailDB : IWorksheetReadable<DovetailDB> {

	public static int FirstRow => 17;

	public static int RowStep => 1;

	public int LineNumber { get; set; }
	public int Qty { get; set; }
	public double Height { get; set; }
	public double Width { get; set; }
	public double Depth { get; set; }
	public string Logo { get; set; } = string.Empty;
	public string Scoop { get; set; } = string.Empty;
	public string Note { get; set; } = string.Empty;
	public decimal UnitPrice { get; set; }
	public decimal ExtPrice { get; set; }

	public static DovetailDB ReadFromWorksheet(Worksheet worksheet, int row) {
		int lineNum = (int)worksheet.GetRangeValueOrDefault($"A{row}", 0.0);
		int qty = (int)worksheet.GetRangeValueOrDefault($"B{row}", 0.0);
		var height = worksheet.GetRangeValueOrDefault($"E{row}", 0.0);
		var width = worksheet.GetRangeValueOrDefault($"F{row}", 0.0);
		var depth = worksheet.GetRangeValueOrDefault($"G{row}", 0.0);
		var logo = worksheet.GetRangeValueOrDefault($"H{row}", "");
		var scoop = worksheet.GetRangeValueOrDefault($"I{row}", "");
		var note = worksheet.GetRangeValueOrDefault($"J{row}", "");
		var unitPrice = (decimal)worksheet.GetRangeValueOrDefault($"O{row}", 0.0);
		var extPrice = (decimal)worksheet.GetRangeValueOrDefault($"P{row}", 0.0);
		return new() {
			LineNumber = lineNum,
			Qty = qty,
			Height = height,
			Width = width,
			Depth = depth,
			Logo = logo,
			Scoop = scoop,
			Note = note,
			UnitPrice = unitPrice,
			ExtPrice = extPrice
		};
	}

}

