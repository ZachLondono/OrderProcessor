using ApplicationCore.Features.Orders.Shared.Domain;
using Domain.ValueObjects;

namespace ApplicationCore.Tests.Unit.Orders.Products;

public record TestToeType : ToeType {
    public TestToeType(Dimension height) : base(height, height, "") {

    }
}