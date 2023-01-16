using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain;

public class LegLevelers : IToeType {
    public Dimension ToeHeight { get; }
    public Dimension HeightAdjustment => ToeHeight;
    public string PSIParameter => "2";
    public LegLevelers(Dimension toeHeight) {
        ToeHeight = toeHeight;
    }
}
