using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Tests.Unit.Orders.Products;

public record TestToeType : ToeType {
    public TestToeType(Dimension height) : base(height, height, "") {
        
    }
}