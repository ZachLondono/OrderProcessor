using CsvHelper.Configuration;
using CsvHelper;
using CsvHelper.TypeConversion;
using System.Numerics;

namespace ApplicationCore.Features.CNC.CSV;

internal class NumberConverter<TNumber> : DefaultTypeConverter where TNumber : INumber<TNumber>
{

    public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {

        if (string.IsNullOrWhiteSpace(text)) return TNumber.Zero;
        return TNumber.Parse(text, null);

    }

}