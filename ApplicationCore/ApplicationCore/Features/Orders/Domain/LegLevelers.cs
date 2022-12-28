using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Domain;

public class LegLevelers : IToeType {
    public Dimension ToeHeight { get; }
    public Dimension HeightAdjustment => ToeHeight;
    public LegLevelers(Dimension toeHeight) {
        ToeHeight = toeHeight;
    }
}
