using ApplicationCore.Features.Orders.Shared.Domain.Excel;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadHafeleDBSpreadsheetOrderData;

public class WorkbookOrderData {

    public required ContactInformation ContactInformation { get; set; }
    public required OrderDetails OrderDetails{ get; set; }
    public required GlobalDrawerSpecs GlobalDrawerSpecs{ get; set; }
    public required LineItem[] Items { get; set; }
    public required string OrderComments { get; set; }

    public static WorkbookOrderData? ReadWorkbook(Workbook workbook) {

        Worksheet? orderSheet = (Worksheet?)workbook.Sheets["Dovetail"];
        if (orderSheet is null) {
            return null;
        }

        var contact = ContactInformation.ReadFromSheet(orderSheet);
        var details = OrderDetails.ReadFromSheet(orderSheet);
        var specs = GlobalDrawerSpecs.ReadFromSheet(orderSheet);
        string orderComments = orderSheet.GetRangeValueOrDefault("N12", "");

        List<LineItem> items = new();

        int rowOffset = 1;
        while (true) {

            if (orderSheet.GetRangeOffsetValueOrDefault("DovetailQtyCol", 0.0, rowOffset) == 0) {
                break;
            }

            var item = LineItem.ReadFromSheet(orderSheet, rowOffset);
            items.Add(item);

            rowOffset ++;

        }

        if (orderSheet is not null) _ = Marshal.ReleaseComObject(orderSheet);

        return new() {
            ContactInformation = contact,
            OrderDetails = details,
            GlobalDrawerSpecs = specs,
            Items = items.ToArray(),
            OrderComments = orderComments
        };

    }

}
