using ApplicationCore.Features.ProductPlanner.Domain;

namespace ApplicationCore.Features.ProductPlanner.Contracts;

public class PPMaterial {

    public string Material { get; }
    public string Color { get; }
    public double Thickness { get; }
    public PPUnits Units { get; }

    public PPMaterial(string material, string color, double thickness = 0, PPUnits units = PPUnits.Millimeters) {
        Material = material;
        Color = color;
        Thickness = thickness;
        Units = units;
    }

    public override string ToString() => $"MATL({Material};{Color};{Thickness}{UnitStr()})";
    private string UnitStr() => Units switch {
        PPUnits.Inches => "in",
        PPUnits.Millimeters => "mm",
        _ => ""
    };
}