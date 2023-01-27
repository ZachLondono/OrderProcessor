using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public class Notched : IToeType {
    public Dimension ToeHeight => Dimension.FromMillimeters(102);
    public Dimension HeightAdjustment => Dimension.Zero;
    public string PSIParameter => "0";
}
