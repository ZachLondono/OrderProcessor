using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain;

public class Notched : IToeType {
    public Dimension ToeHeight { get; }
    public Dimension HeightAdjustment => Dimension.Zero;
    public string PSIParameter => "0";
    public Notched(Dimension toeHeight) {
        ToeHeight = toeHeight;
    }
}
