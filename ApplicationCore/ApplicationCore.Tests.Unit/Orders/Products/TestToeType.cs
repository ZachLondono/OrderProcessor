using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Tests.Unit.Orders.Products;

public class TestToeType : IToeType {
    public Dimension ToeHeight { get; }
    public Dimension HeightAdjustment { get; }
    public string PSIParameter => string.Empty;
    public TestToeType(Dimension height) {
        ToeHeight = height;
        HeightAdjustment = height;
    }
}