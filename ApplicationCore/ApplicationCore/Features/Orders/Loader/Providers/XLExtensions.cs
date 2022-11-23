using ClosedXML.Excel;
namespace ApplicationCore.Features.Orders.Loader.Providers;

internal static class XLExtensions {

	public static IXLCell GetOffsetCell(this IXLCell relative, int rowOffset = 0, int colOffset = 0) {
		return relative.Address.Worksheet.Cell(relative.Address.RowNumber + rowOffset, relative.Address.ColumnNumber + colOffset);
	}

	public static string ReadString(this IXLCell cell) {

		if (cell.HasFormula) return (string) cell.CachedValue;

		bool wasRead = cell.TryGetValue<string>(out string val);

		if (wasRead) return val;

		return (string) cell.CachedValue;

	}

	public static double ReadDouble(this IXLCell cell) {

		if (cell.HasFormula) {

			if (cell.DataType == XLDataType.Number) return (double) cell.CachedValue;

			if (double.TryParse(cell.CachedValue.ToString() ?? "0", out double parsed)) return parsed;

			return default;

		}

		bool wasRead = cell.TryGetValue<double>(out double val);

		if (wasRead) return val;

		throw new InvalidDataException($"Could not read double from cell {cell}");

	}

	public static decimal ReadDecimal(this IXLCell cell) {

		if (cell.HasFormula) {

			if (cell.DataType == XLDataType.Number) return (decimal)cell.CachedValue;

			if (decimal.TryParse(cell.CachedValue.ToString() ?? "0", out decimal parsed)) return parsed;

			return default;

		}

		bool wasRead = cell.TryGetValue<decimal>(out decimal val);

		if (wasRead) return val;

        throw new InvalidDataException($"Could not read decimal from cell {cell}");

    }

	public static int ReadInt(this IXLCell cell) {

		if (cell.HasFormula) {

			if (cell.DataType == XLDataType.Number) return (int)cell.CachedValue;

			if (int.TryParse(cell.CachedValue.ToString() ?? "0", out int parsed)) return parsed;

			return default;

		}

		bool wasRead = cell.TryGetValue<int>(out int val);

		if (wasRead) return val;

        throw new InvalidDataException($"Could not read integer from cell {cell}");

    }

	public static bool ValueIsNullOrWhitespace(this IXLCell cell) {

		if (cell.HasFormula) return string.IsNullOrWhiteSpace(cell.CachedValue.ToString());

		if (cell.TryGetValue(out string val)) return string.IsNullOrWhiteSpace(val);

		return true;

	}

}
