using ClosedXML.Excel;

namespace OrderLoading.LoadHafeleMDFDoorSpreadsheetOrderData.ReadOrderFile;

public static class ExcelExtensions {

    public static T GetValueOrDefault<T>(this IXLCell cell, T defaultVal) {

        try {

            return cell.GetValue<T>();

        } catch {

            return defaultVal;

        }

    }

}

