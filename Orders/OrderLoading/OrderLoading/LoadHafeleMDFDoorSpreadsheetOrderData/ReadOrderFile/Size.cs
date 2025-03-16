using ClosedXML.Excel;

namespace OrderLoading.LoadHafeleMDFDoorSpreadsheetOrderData.ReadOrderFile;

public class Size {

    public required int LineNumber { get; init; }
    public required string Type { get; init; }
    public required int Qty { get; init; }
    public required double Width { get; init; }
    public required double Height { get; init; }
    public required string SpecialInstructions { get; init; }
    public required decimal UnitPrice { get; init; }
    public required decimal ExtendedPrice { get; init; }
    public required double LeftStile { get; init; }
    public required double RightStile { get; init; }
    public required double TopRail { get; init; }
    public required double BottomRail { get; init; }
    public required double Rail3 { get; init; }
    public required double Rail4 { get; init; }
    public required double Rail5 { get; init; }
    public required double Panel1Height { get; init; }
    public required double Panel2Height { get; init; }
    public required double Panel3Height { get; init; }

    public static Size[] LoadFromWorkbook(XLWorkbook workbook) {

        var sheet = workbook.Worksheet("Sizes");
        var sizes = new List<Size>();

        int i = 0;
        while (true) {

            i++;

            var qtyCell = sheet.Cell($"Qty_Col").CellBelow(i);

            if (qtyCell.WorksheetRow().RowNumber() == 104) {
                break;
            }

            if (qtyCell.IsEmpty()) {
                continue;
            }

            sizes.Add(new Size {
                LineNumber = sheet.Cell("Line_Col").CellBelow(i).GetValue<int>(),
                Type = sheet.Cell("Type_Col").CellBelow(i).GetValue<string>(),
                Qty = qtyCell.GetValue<int>(),
                Width = sheet.Cell("Width_Col").CellBelow(i).GetValue<double>(),
                Height = sheet.Cell("Height_Col").CellBelow(i).GetValue<double>(),
                SpecialInstructions = sheet.Cell("Special_Instructions_Col").CellBelow(i).GetValue<string>(),
                UnitPrice = sheet.Cell("Unit_Price_Col").CellBelow(i).GetValue<decimal>(),
                ExtendedPrice = sheet.Cell("Ext_Price_Col").CellBelow(i).GetValue<decimal>(),
                LeftStile = sheet.Cell("Left_Stile_Col").CellBelow(i).GetValue<double>(),
                RightStile = sheet.Cell("Right_Stile_Col").CellBelow(i).GetValue<double>(),
                TopRail = sheet.Cell("Top_Rail_Col").CellBelow(i).GetValue<double>(),
                BottomRail = sheet.Cell("Bottom_Rail_Col").CellBelow(i).GetValue<double>(),
                Rail3 = sheet.Cell("Rail_3_Col").CellBelow(i).GetValueOrDefault<double>(0),
                Rail4 = sheet.Cell("Rail_4_Col").CellBelow(i).GetValueOrDefault<double>(0),
                Rail5 = sheet.Cell("Rail_5_Col").CellBelow(i).GetValueOrDefault<double>(0),
                Panel1Height = sheet.Cell("Panel_1_H_Col").CellBelow(i).GetValueOrDefault<double>(0),
                Panel2Height = sheet.Cell("Panel_2_H_Col").CellBelow(i).GetValueOrDefault<double>(0),
                Panel3Height = sheet.Cell("Panel_3_H_Col").CellBelow(i).GetValueOrDefault<double>(0)
            });

        }

        return [.. sizes];

    }

}
