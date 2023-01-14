using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain;

public interface IToeType {
    Dimension ToeHeight { get; }
    Dimension HeightAdjustment { get; }
    string PSIParameter { get; }
}
