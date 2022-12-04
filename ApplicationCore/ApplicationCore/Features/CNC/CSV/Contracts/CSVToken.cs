using ApplicationCore.Features.CNC.CSV.Converters;
using CsvHelper.Configuration.Attributes;

namespace ApplicationCore.Features.CNC.CSV.Contracts;

public record CSVToken {

	public string JobName { get; init; } = string.Empty;

	public string ProductID { get; init; } = string.Empty;

	public string PartID { get; init; } = string.Empty;

	public string PartName { get; init; } = string.Empty;

	[TypeConverter(typeof(NumberConverter<int>))]
	public int Quantity { get; init; }

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

    public string OffsetSide { get; init; } = string.Empty;

    public string ToolNumber { get; init; } = string.Empty;

    [TypeConverter(typeof(NumberConverter<double>))]
    public double ToolDiameter { get; init; }

    [TypeConverter(typeof(NumberConverter<int>))]
    public int SequenceNumber { get; init; }

    [TypeConverter(typeof(NumberConverter<double>))]
    public double FeedSpeed { get; init; }

    [TypeConverter(typeof(NumberConverter<double>))]
    public double SpindleSpeed { get; init; }

	public string Filename { get; init; } = string.Empty;

	public string Face6Filename { get; init; } = string.Empty;

	public string Face6Flag { get; init; } = string.Empty;

	public string Material { get; init; } = string.Empty;

	[TypeConverter(typeof(BooleanTypeConverter))]
	public bool Graining { get; init; }

	[TypeConverter(typeof(NumberConverter<int>))]
	public int Rotation { get; init; }

	[TypeConverter(typeof(BooleanTypeConverter))]
	public bool IslandPart { get; init; }

	[TypeConverter(typeof(BooleanTypeConverter))]
	public bool SmallPart { get; init; }

}
