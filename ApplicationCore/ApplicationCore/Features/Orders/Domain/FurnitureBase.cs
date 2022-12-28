using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Domain;

public class FurnitureBase : IToeType {
    public Dimension ToeHeight { get; }
    public Dimension HeightAdjustment => Dimension.Zero;
    public FurnitureBase(Dimension toeHeight) {
        ToeHeight = toeHeight;
    }
}