using Microsoft.Office.Interop.Excel;

namespace OrderLoading.LoadDoweledDBSpreadsheetOrderData.Models;

public static class ExcelExtensions {

	public static string GetStringValue(this Microsoft.Office.Interop.Excel.Range range) {

		if (range is null) return string.Empty;
		if (range.Value2 is null) return string.Empty;

		if (range.Value2 is string val) {
			return val;
		}

		return range.Value2.ToString();

	}

	public static T GetValue<T>(this Microsoft.Office.Interop.Excel.Range range, T defaultValue) {

		if (range is null) return defaultValue;
		if (range.Value2 is null) return defaultValue;

		if (range.Value2 is T val) {
			return val;
		}

		return defaultValue;

	}

	public static T GetRangeValue<T>(this Worksheet worksheet, string rangeAddress, T defaultValue, int rowOffset = 0, int colOffset = 0) {

		try {

			var rng = worksheet.Range[rangeAddress];
			return rng.Offset[rowOffset, colOffset].GetValue(defaultValue);

		} catch {

			return defaultValue;

		}

	}

	public static string GetRangeStringValue(this Worksheet worksheet, string rangeAddress, int rowOffset = 0, int colOffset = 0) {

		try {

			var rng = worksheet.Range[rangeAddress];
			return rng.Offset[rowOffset, colOffset].GetStringValue();

		} catch {

			return string.Empty;

		}

	}

	public static DateTime GetRangeDateTimeValue(this Worksheet worksheet, string rangeAddress, int rowOffset = 0, int colOffset = 0, DateTime defaultValue = default) {

		try {

			var rng = worksheet.Range[rangeAddress];
			var value = rng.Offset[rowOffset, colOffset].GetStringValue();

			return DateTime.FromOADate(double.Parse(value));

		} catch {

			return defaultValue;

		}

	}

}
