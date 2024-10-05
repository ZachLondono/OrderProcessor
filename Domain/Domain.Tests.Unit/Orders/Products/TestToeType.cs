using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace Domain.Tests.Unit.Orders.Products;

public record TestToeType : ToeType {
    public TestToeType(Dimension height) : base(height, height, "") {

    }
}