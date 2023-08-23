using ApplicationCore.Features.Orders.Shared.Domain.Excel;
using Microsoft.Office.Interop.Excel;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetOrderSpreadsheetOrderData.Models;

internal record ClosetPart : IWorksheetReadable<ClosetPart> { 

	public int Number { get; init; }
	public string Item { get; init; } = string.Empty;
	public int Qty { get; init; }
	public double Width { get; init; }
	public double Length { get; init; }
	public string WallMount { get; init; } = string.Empty;
	public string Finished { get; init; } = string.Empty;
	public string RoomName { get; init; } = string.Empty;
	public string Comment { get; init; } = string.Empty;
	public decimal UnitPrice { get; init; }
	public decimal ExtPrice  { get; init; }

	public static int FirstRow => 2;

	public static int RowStep => 1;

	public static ClosetPart ReadFromWorksheet(Worksheet worksheet, int row) {
		int number = worksheet.GetRangeValueOrDefault($"A{row}", 0);
		var item = worksheet.GetRangeValueOrDefault($"B{row}", "");
		var qty = worksheet.GetRangeValueOrDefault($"C{row}", 0);
		var width = worksheet.GetRangeValueOrDefault($"D{row}", 0.0);
		var length = worksheet.GetRangeValueOrDefault($"E{row}", 0.0);
		var wallMount = worksheet.GetRangeValueOrDefault($"F{row}", "");
		var finished = worksheet.GetRangeValueOrDefault($"G{row}", "");
		var roomName = worksheet.GetRangeValueOrDefault($"H{row}", "");
		var comment = worksheet.GetRangeValueOrDefault($"I{row}", "");
		var unitPrice = worksheet.GetRangeValueOrDefault($"J{row}", 0m);
		var extPrice = worksheet.GetRangeValueOrDefault($"K{row}", 0m);
		return new() {
			Number = number,
			Item = item,
			Qty = qty,
			Width = width,
			Length = length,
			WallMount = wallMount,
			Finished = finished,
			RoomName = roomName,
			Comment = comment,
			UnitPrice = unitPrice,
			ExtPrice = extPrice
		};
	}

}
