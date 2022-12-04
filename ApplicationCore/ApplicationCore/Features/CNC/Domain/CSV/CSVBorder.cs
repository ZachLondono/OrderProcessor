using CsvHelper.Configuration.Attributes;

namespace ApplicationCore.Features.CNC.Domain.CSV;

internal record CSVBorder {

    public string JobName { get; init; } = string.Empty;
    
    public string ProductID { get; init; } = string.Empty;
    
    public string PartID { get; init; } = string.Empty;
    
    public string PartName { get; init; } = string.Empty;

    [TypeConverter(typeof(NumberConverter<int>))]
    public int Quantity { get; init; }
	
    [TypeConverter(typeof(NumberConverter<double>))]
	public double StartX { get; init; }
	
    [TypeConverter(typeof(NumberConverter<double>))]
	public double StartY { get; init; }
	
    [TypeConverter(typeof(NumberConverter<double>))]
	public double StartZ { get; init; }

    public string Filename { get; init; } = string.Empty;
    
    public string Face6Filename { get; init; } = string.Empty;
    
    public string Face6Flag { get; init; } = string.Empty;
    
    public string Material { get; init; } = string.Empty;

    [TypeConverter(typeof(BooleanTypeConverter))]
    public bool Graining { get; init; }
    
    public string Rotation { get; init; } = string.Empty;

	[TypeConverter(typeof(BooleanTypeConverter))]
	public bool IslandPart { get; init; }

	[TypeConverter(typeof(BooleanTypeConverter))]
	public bool SmallPart { get; init; }

}
