using ApplicationCore.Shared;
using Domain.Excel;
using Domain.ValueObjects;
using Microsoft.Office.Interop.Excel;
using System.Diagnostics;
using ExcelApp = Microsoft.Office.Interop.Excel.Application;

namespace ApplicationCore.Features.FivePieceOrderRelease;

internal class FivePieceOrderSheetParser(IWindowFocuser windowFocuser) {

    private readonly IWindowFocuser _windowFocuser = windowFocuser;

    public FivePieceOrder LoadOrderFromFile(string filePath) {

        var app = GetExcelInstance();

        try {

            if (app is null) {
                throw new InvalidOperationException("Door order was not open");
            }

            app.ScreenUpdating = false;
            app.DisplayAlerts = false;
            app.Calculation = XlCalculation.xlCalculationManual;

            var workbooks = app.Workbooks;
            var workbook = workbooks.Open(filePath);
            var worksheets = workbook.Worksheets;

            Worksheet sheet = worksheets["Order"];

            var lines = ReadLinesFromWorksheet(sheet);

            var orderDate = ReadDateTimeFromWorkbook(sheet, "OrderDate");
            var dueDate = ReadDateTimeFromWorkbook(sheet, "DueDate");
            var company = sheet.Range["Company"].Value2;
            var trackingNumber = sheet.Range["TrackingNumber"].Value2;
            var jobName = sheet.Range["JobName"].Value2;
            var material = sheet.Range["Material"].Value2;

            return new FivePieceOrder(orderDate,
                                      dueDate,
                                      company,
                                      trackingNumber,
                                      jobName,
                                      material,
                                      lines);

        } finally {

            if (app is not null) {
                app.ScreenUpdating = true;
                app.DisplayAlerts = true;
                app.Calculation = XlCalculation.xlCalculationAutomatic;
            }

            _windowFocuser.TryToSetMainWindowFocus();

        }

    }

    private static IEnumerable<LineItem> ReadLinesFromWorksheet(Worksheet sheet) {

        var dimensionParse = GetDimensionParserFromSheet(sheet);
        List<LineItem> lines = [];
        int row = 1;

        while (true) {

            if (string.IsNullOrEmpty(sheet.Range["QtyStart"].Offset[row].Value2.ToString())) {
                break;
            }

            var line = ReadLineItemFromRow(row, sheet, dimensionParse);
            lines.Add(line);

            row++;

        }

        return lines;

    }

    private static Func<double, Dimension> GetDimensionParserFromSheet(Worksheet sheet) {

        string unitsStr = sheet.Range["Units"].Value2;

        return unitsStr switch {
            "Millimeters" => Dimension.FromMillimeters,
            "Inches" => Dimension.FromInches,
            _ => throw new InvalidOperationException("Unexpected unit format")
        };

    }

    private static LineItem ReadLineItemFromRow(int row, Worksheet worksheet, Func<double, Dimension> dimensionParse) {

        int partNum = worksheet.Range["PartNumStart"].Offset[row].Value2;
        string description = worksheet.Range["DescriptionStart"].Offset[row].Value2;
        int line = worksheet.Range["LineStart"].Offset[row].Value2;
        int qty = worksheet.Range["QtyStart"].Offset[row].Value2;

        double widthDbl = worksheet.Range["WidthStart"].Offset[row].Value2;
        Dimension width = dimensionParse(widthDbl);

        double heightDbl = worksheet.Range["HeightStart"].Offset[row].Value2;
        Dimension height = dimensionParse(heightDbl);

        double leftDbl = worksheet.Range["LeftStileStart"].Offset[row].Value2;
        Dimension leftStile = dimensionParse(leftDbl);

        double rightDbl = worksheet.Range["RightStileStart"].Offset[row].Value2;
        Dimension rightStile = dimensionParse(rightDbl);

        double topDbl = worksheet.Range["TopRailStart"].Offset[row].Value2;
        Dimension topRail = dimensionParse(topDbl);

        double bottomDbl = worksheet.Range["BottomRailStart"].Offset[row].Value2;
        Dimension bottomRail = dimensionParse(bottomDbl);

        string specialFeatures = worksheet.Range["SpecialFeaturesStart"].Offset[row].Value2;

        return new LineItem(partNum,
                            description,
                            line,
                            qty,
                            width,
                            height,
                            specialFeatures,
                            leftStile,
                            rightStile,
                            topRail,
                            bottomRail);

    }

    private static ExcelApp? GetExcelInstance() {

        var allProcesses = Process.GetProcesses();

        foreach (var process in allProcesses) {

            nint winHandle = process.MainWindowHandle;

            var retriever = new ExcelApplicationRetriever((int)winHandle);

            if (retriever.xl is not null) {
                return retriever.xl;
            }

        }

        return null;

    }

    private static DateTime ReadDateTimeFromWorkbook(Worksheet sheet, string rangeName) {

        try {

            var rng = sheet.Range[rangeName];

            if (rng.Value2 is double oaDate) {
                return DateTime.FromOADate(oaDate);
            }

            return DateTime.Today;

        } catch {

            return DateTime.Today;

        }

    }

}
