using Microsoft.Office.Interop.Excel;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetOrderSpreadsheetOrderData.Models;

internal record DovetailDBHeader(string BoxMaterial, string BottomMaterial, string Notch, string Clips, bool PostFinish, bool SandFlush) {

	public static DovetailDBHeader ReadFromWorksheet(Worksheet worksheet) {
		throw new NotImplementedException();
	}

}

