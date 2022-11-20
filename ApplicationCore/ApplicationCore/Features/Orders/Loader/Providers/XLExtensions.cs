using ClosedXML.Excel;
namespace ApplicationCore.Features.Orders.Loader.Providers;

internal static class XLExtensions {

	public static IXLCell GetOffsetCell(this IXLCell relative, int rowOffset = 0, int colOffset = 0) {
		return relative.Address.Worksheet.Cell(relative.Address.RowNumber + rowOffset, relative.Address.ColumnNumber + colOffset);
	}

}
