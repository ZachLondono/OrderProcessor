using Microsoft.Office.Interop.Excel;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetOrderSpreadsheetOrderData.Models;

internal interface IWorksheetReadable<T> {

	public static abstract T ReadFromWorksheet(Worksheet worksheet, int row);

	public static abstract int FirstRow { get; }

	public static abstract int RowStep { get; }

}
