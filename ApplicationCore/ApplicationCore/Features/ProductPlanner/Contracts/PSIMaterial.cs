namespace ApplicationCore.Features.ProductPlanner.Domain;

public class PSIMaterial {
    public required string Material { get; set; }
    public required string Color { get; set; }
    public required double Thickness { get; set; }
    public required PSIUnits Units { get; set; }
    public override string ToString() => $"MATL({Material};{Color};{Thickness}{UnitStr()})";
    private string UnitStr() => Units switch {
        PSIUnits.Inches => "in",
        PSIUnits.Millimeters => "mm",
        _ => ""
    };
}