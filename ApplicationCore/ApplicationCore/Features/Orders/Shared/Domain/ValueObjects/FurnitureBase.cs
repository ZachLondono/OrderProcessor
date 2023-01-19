using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public class FurnitureBase : IToeType {
    public Dimension ToeHeight { get; }
    public Dimension HeightAdjustment => Dimension.Zero;
    public string PSIParameter => "1";
    public FurnitureBase(Dimension toeHeight) {
        ToeHeight = toeHeight;
    }
}