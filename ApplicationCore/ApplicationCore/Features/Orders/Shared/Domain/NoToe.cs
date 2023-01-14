using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain;

public class NoToe : IToeType {
    public Dimension ToeHeight => Dimension.Zero;
    public Dimension HeightAdjustment => Dimension.Zero;
    public string PSIParameter => "3";
}
