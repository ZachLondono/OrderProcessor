using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain;

public record ToeType {

    public Dimension ToeHeight { get; }
    public Dimension HeightAdjustment { get; }
    public string PSIParameter { get; }

    protected ToeType(Dimension toeHeight, Dimension heightAdjustment, string psiParameter) {
        ToeHeight = toeHeight;
        HeightAdjustment = heightAdjustment;
        PSIParameter = psiParameter;
    }

    public static ToeType Notched => new(Dimension.FromMillimeters(102), Dimension.Zero, "0");
    public static ToeType FurnitureBase => new(Dimension.FromMillimeters(102), Dimension.Zero, "1");
    public static ToeType LegLevelers => new(Dimension.FromMillimeters(102), Dimension.FromMillimeters(102), "2");
    public static ToeType NoToe => new(Dimension.Zero, Dimension.Zero, "3");

    public static ToeType FromPSIParameter(string value) => value switch {
        "0" => Notched,
        "1" => FurnitureBase,
        "2" => LegLevelers,
        "3" => NoToe,
        _ => throw new ArgumentException($"Invalid psi toe type parameter '{value}'", nameof(value))
    };

}