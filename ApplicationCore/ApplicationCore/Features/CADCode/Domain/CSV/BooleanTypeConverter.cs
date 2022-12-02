using CsvHelper.Configuration;
using CsvHelper;
using CsvHelper.TypeConversion;

namespace ApplicationCore.Features.CADCode.Domain.CSV;

internal class BooleanTypeConverter : DefaultTypeConverter {

	public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData) => text.ToLower() switch {
		"yes" or "y" => true,
		"no" or "n" or "" => false,
		_ => false
	};

}