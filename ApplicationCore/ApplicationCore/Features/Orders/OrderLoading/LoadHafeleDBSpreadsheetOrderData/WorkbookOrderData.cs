using ApplicationCore.Features.Orders.Shared.Domain.Excel;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadHafeleDBSpreadsheetOrderData;

public class WorkbookOrderData {

    public required ContactInformation ContactInformation { get; set; }
    public required OrderDetails OrderDetails { get; set; }
    public required GlobalDrawerSpecs GlobalDrawerSpecs { get; set; }
    public required LineItem[] Items { get; set; }
    public required string OrderComments { get; set; }

    public static WorkbookOrderData? ReadWorkbook(Workbook workbook) {

        Worksheet? dovetailOrderSheet = (Worksheet?)workbook.Sheets["Dovetail"];
        Worksheet? dowelOrderSheet = (Worksheet?)workbook.Sheets["Dowel"];
        if (dovetailOrderSheet is null || dowelOrderSheet is null) {
            return null;
        }

        var contact = ContactInformation.ReadFromSheet(dovetailOrderSheet);
        var details = OrderDetails.ReadFromSheet(dovetailOrderSheet);
        var specs = GlobalDrawerSpecs.ReadFromSheet(dowelOrderSheet);
        string orderComments = dovetailOrderSheet.GetRangeValueOrDefault("N12", "");

        List<LineItem> items = new();

        int rowOffset = 1;
        while (true) {

            if (dowelOrderSheet.GetRangeOffsetValueOrDefault("DowelQtyCol", 0.0, rowOffset) == 0) {
                break;
            }

            var item = LineItem.ReadFromSheet(dowelOrderSheet, rowOffset);
            items.Add(item);

            rowOffset++;

        }

        if (dovetailOrderSheet is not null) _ = Marshal.ReleaseComObject(dovetailOrderSheet);
        if (dowelOrderSheet is not null) _ = Marshal.ReleaseComObject(dowelOrderSheet);

        return new() {
            ContactInformation = contact,
            OrderDetails = details,
            GlobalDrawerSpecs = specs,
            Items = items.ToArray(),
            OrderComments = orderComments
        };

    }

}
