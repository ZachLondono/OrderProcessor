using ClosedXML.Excel;

namespace ApplicationCore.Features.Orders.OrderLoading.Providers.DoorOrderModels {
    internal static class WorkBookExtensions {

        public static T GetValueFromRange<T>(this IXLWorksheet sheet, string range) => sheet.Range(range).FirstCell().GetValue<T>();

    }
}