using CsvHelper.Configuration;
using CsvHelper;
using CsvHelper.TypeConversion;
using ApplicationCore.Features.CADCode.Contracts.Machining;

namespace ApplicationCore.Features.CADCode.Domain.CSV;

internal class OffsetTypeConverter : DefaultTypeConverter {

	public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData) => text switch {
		"I" => OffsetType.Inside,
		"O" => OffsetType.Outside,
		"R" => OffsetType.Right,
		"L" => OffsetType.Left,
		"C" => OffsetType.Center,
		_ => OffsetType.None
	};

}
