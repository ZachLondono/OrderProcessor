using Domain.Excel;
using Microsoft.Office.Interop.Excel;

namespace OrderLoading.LoadClosetOrderSpreadsheetOrderData.Models;

public record CornerShelf : IWorksheetReadable<CornerShelf> {

	public static int FirstRow => 2;

	public static int RowStep => 1;

	public int Number { get; init; }
	public string Item { get; init; } = string.Empty;
	public int Qty { get; init; }
	public double ProductWidth { get; init; }
	public double ProductLength { get; init; }
	public double RightWidth { get; init; }
	public double NotchSideLength { get; init; }
	public string NotchSide { get; init; } = string.Empty;
	public string RoomName { get; init; } = string.Empty;
	public string Comment { get; init; } = string.Empty;
	public double ShelfRadius { get; init; }
	public decimal UnitPrice { get; init; }
	public decimal ExtPrice { get; init; }

	public static CornerShelf ReadFromWorksheet(Worksheet worksheet, int row) {
		int number = (int)worksheet.GetRangeValueOrDefault($"A{row}", 0.0);
		var item = worksheet.GetRangeValueOrDefault($"B{row}", "");
		var qty = (int)worksheet.GetRangeValueOrDefault($"C{row}", 0.0);
		var productWidth = worksheet.GetRangeValueOrDefault($"D{row}", 0.0);
		var productLength = worksheet.GetRangeValueOrDefault($"E{row}", 0.0);
		var rightWidth = worksheet.GetRangeValueOrDefault($"F{row}", 0.0);
		var notchSideLength = worksheet.GetRangeValueOrDefault($"G{row}", 0.0);
		var notchSide = worksheet.GetRangeValueOrDefault($"H{row}", "");
		var roomName = worksheet.GetRangeValueOrDefault($"I{row}", "");
		var comment = worksheet.GetRangeValueOrDefault($"J{row}", "");
		var shelfRadius = worksheet.GetRangeValueOrDefault($"K{row}", 0.0);
		var unitPrice = (decimal)worksheet.GetRangeValueOrDefault($"L{row}", 0.0);
		var extPrice = (decimal)worksheet.GetRangeValueOrDefault($"M{row}", 0.0);
		return new() {
			Number = number,
			Item = item,
			Qty = qty,
			ProductWidth = productWidth,
			ProductLength = productLength,
			RightWidth = rightWidth,
			NotchSide = notchSide,
			NotchSideLength = notchSideLength,
			RoomName = roomName,
			Comment = comment,
			ShelfRadius = shelfRadius,
			UnitPrice = unitPrice,
			ExtPrice = extPrice
		};
	}

}

