using Microsoft.Office.Interop.Excel;

namespace ApplicationCore.Features.Orders.Shared.Domain.Excel;

internal static class WorkBookExtensions {

    public static T GetRangeValueOrDefault<T>(this Worksheet sheet, string rngName, T defaultValue) {

        try {

            return (T) sheet.Range[rngName].Value2;

        } catch {

            return defaultValue;

        }

    }

    public static T GetRangeOffsetValueOrDefault<T>(this Worksheet sheet, string rngName, T defaultValue, int rowOffset = 0, int colOffset = 0) {

        try {

            return (T) sheet.Range[rngName].Offset[rowOffset, colOffset].Value2;

        } catch {

            return defaultValue;

        }

    }

}
