using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Domain;

public class Notched : IToeType {
    public Dimension ToeHeight { get; }
    public Dimension HeightAdjustment => Dimension.Zero;
    public Notched(Dimension toeHeight) {
        ToeHeight = toeHeight;
    }
}
