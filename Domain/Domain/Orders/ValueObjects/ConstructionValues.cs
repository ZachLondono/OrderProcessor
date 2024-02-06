namespace Domain.Orders.ValueObjects;

public class ConstructionValues {

    public double FrontBackWidthAdjustment { get; set; }

    public double SideLengthAdjustment { get; set; }

    public double BottomSizeAdjustment { get; set; }

    public Dictionary<string, string> MaterialCodes { get; set; } = new();

}
