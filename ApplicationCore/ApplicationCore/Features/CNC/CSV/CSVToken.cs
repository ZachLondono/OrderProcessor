using ApplicationCore.Features.CNC.Contracts.Machining;
using CsvHelper.Configuration.Attributes;

namespace ApplicationCore.Features.CNC.CSV;

internal record CSVToken
{

    public string MachiningToken { get; init; } = string.Empty;

    [TypeConverter(typeof(NumberConverter<double>))]
    public double StartX { get; init; }

    [TypeConverter(typeof(NumberConverter<double>))]
    public double StartY { get; init; }

    [TypeConverter(typeof(NumberConverter<double>))]
    public double StartZ { get; init; }

    [TypeConverter(typeof(NumberConverter<double>))]
    public double EndX { get; init; }

    [TypeConverter(typeof(NumberConverter<double>))]
    public double EndY { get; init; }

    [TypeConverter(typeof(NumberConverter<double>))]
    public double EndZ { get; init; }

    [TypeConverter(typeof(NumberConverter<double>))]
    public double CenterX { get; init; }

    [TypeConverter(typeof(NumberConverter<double>))]
    public double CenterY { get; init; }

    [TypeConverter(typeof(NumberConverter<double>))]
    public double PocketX { get; init; }

    [TypeConverter(typeof(NumberConverter<double>))]
    public double PocketY { get; init; }

    [TypeConverter(typeof(NumberConverter<double>))]
    public double Radius { get; init; }

    [TypeConverter(typeof(NumberConverter<int>))]
    public int Passes { get; init; }

    [TypeConverter(typeof(OffsetTypeConverter))]
    public OffsetType OffsetSide { get; init; } = OffsetType.None;

    public string ToolNumber { get; init; } = string.Empty;

    [TypeConverter(typeof(NumberConverter<double>))]
    public double ToolDiameter { get; init; }

    [TypeConverter(typeof(NumberConverter<int>))]
    public int SequenceNumber { get; init; }

    [TypeConverter(typeof(NumberConverter<double>))]
    public double FeedSpeed { get; init; }

    [TypeConverter(typeof(NumberConverter<double>))]
    public double SpindleSpeed { get; init; }

}
