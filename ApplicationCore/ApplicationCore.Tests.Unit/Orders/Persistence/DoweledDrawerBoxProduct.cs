using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Tests.Unit.Orders.Persistence;

public partial class OrderTests {

    [Fact]
    public void InsertOrderWithDoweledDrawerBox() {
        var db = new DoweledDrawerBoxProduct(Guid.NewGuid(), 0M, 1, "", 1,
                                            Dimension.FromInches(4.125), Dimension.FromInches(21), Dimension.FromInches(21),
                                            new("", Dimension.Zero, false), new("", Dimension.Zero, false), new("", Dimension.Zero, false), new("", Dimension.Zero, false),
                                            false, Dimension.Zero);
        InsertAndQueryOrderWithProduct(db);
    }

    [Fact]
    public void DeleteOrderWithDoweledDrawerBox() {
        var db = new DoweledDrawerBoxProduct(Guid.NewGuid(), 0M, 1, "", 1,
                                            Dimension.FromInches(4.125), Dimension.FromInches(21), Dimension.FromInches(21),
                                            new("", Dimension.Zero, false), new("", Dimension.Zero, false), new("", Dimension.Zero, false), new("", Dimension.Zero, false),
                                            false, Dimension.Zero);
        InsertAndDeleteOrderWithProduct(db);
    }

}



