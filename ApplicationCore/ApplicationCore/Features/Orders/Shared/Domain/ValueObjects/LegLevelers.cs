using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public class LegLevelers : IToeType {
    public Dimension ToeHeight => Dimension.FromMillimeters(102);
    public Dimension HeightAdjustment => ToeHeight;
    public string PSIParameter => "2";
}
