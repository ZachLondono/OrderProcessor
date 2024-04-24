using Microsoft.Office.Interop.Excel;

namespace OrderLoading.LoadClosetOrderSpreadsheetOrderData.Models;

public interface IWorksheetReadable<T> {

	public static abstract T ReadFromWorksheet(Worksheet worksheet, int row);

	public static abstract int FirstRow { get; }

	public static abstract int RowStep { get; }

}
