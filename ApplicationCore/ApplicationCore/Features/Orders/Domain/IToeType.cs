using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Domain;

public interface IToeType {
    Dimension ToeHeight { get; }
    Dimension HeightAdjustment { get; }
}
