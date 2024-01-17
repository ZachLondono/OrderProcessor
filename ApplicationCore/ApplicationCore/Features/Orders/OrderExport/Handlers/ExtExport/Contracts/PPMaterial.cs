using ApplicationCore.Features.Orders.OrderExport.Handlers.ExtExport.Domain;

namespace ApplicationCore.Features.Orders.OrderExport.Handlers.ExtExport.Contracts;

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

    public override string ToString() => $"MATL({Material};{ShortenedColor};{Thickness}{UnitStr()})";

    public string ShortenedColor => (Color.Length > 20 ? Color[..20] : Color).Trim();

    private string UnitStr() => Units switch {
        PPUnits.Inches => "in",
        PPUnits.Millimeters => "mm",
        _ => ""
    };
}