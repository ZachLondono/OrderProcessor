using ApplicationCore.Features.Orders.Shared.Domain.Excel;
using Microsoft.Office.Interop.Excel;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadHafeleDBSpreadsheetOrderData;

public class WorkbookOrderData {

    public required ContactInformation ContactInformation { get; set; }
    public required OrderDetails OrderDetails{ get; set; }
    public required GlobalDrawerSpecs GlobalDrawerSpecs{ get; set; }
    public required LineItem[] Items { get; set; }
    public required string OrderCommnets { get; set; }

    public static WorkbookOrderData? ReadWorkbook(Workbook workbook) {

        Worksheet? orderSheet = (Worksheet?)workbook.Sheets["Dovetail"];
        if (orderSheet is null) {
            return null;
        }

        var contact = ContactInformation.ReadFromSheet(orderSheet);
        var details = OrderDetails.ReadFromSheet(orderSheet);
        var specs = GlobalDrawerSpecs.ReadFromSheet(orderSheet);
        string orderCommnets = orderSheet.GetRangeValueOrDefault("N12", "");

        List<LineItem> items = new();

        int rowOffset = 1;
        while (true) {

            if (string.IsNullOrEmpty(orderSheet.GetRangeOffsetValueOrDefault("DovetailQtyCol", string.Empty, rowOffset))) {
                break;
            }

            items.Add(LineItem.ReadFromSheet(orderSheet, rowOffset));

            rowOffset ++;

        }

        return new() {
            ContactInformation = contact,
            OrderDetails = details,
            GlobalDrawerSpecs = specs,
            Items = items.ToArray(),
            OrderCommnets = orderCommnets
        };

    }

}
