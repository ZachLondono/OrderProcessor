using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products.DrawerBoxes;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Tests.Unit.Orders.Persistence;

public partial class OrderTests {

    [Fact]
    public void InsertOrderWithDovetailDrawerBox() {
        var db = new DovetailDrawerBoxProduct(Guid.NewGuid(), 0M, 1, "", 1, Dimension.FromInches(4.125), Dimension.FromInches(21), Dimension.FromInches(21), "", new Dictionary<string, string>(), new("MatA", "MatB", "MatC", "MatD", "Clips", "Notches", "Accessory", LogoPosition.None));
        InsertAndQueryOrderWithProduct(db);
    }

    [Fact]
    public void DeleteOrderWithDovetailDrawerBox() {
        var db = new DovetailDrawerBoxProduct(Guid.NewGuid(), 0M, 1, "", 1, Dimension.FromInches(4.125), Dimension.FromInches(21), Dimension.FromInches(21), "", new Dictionary<string, string>(), new("MatA", "MatB", "MatC", "MatD", "Clips", "Notches", "Accessory", LogoPosition.None));
        InsertAndDeleteOrderWithProduct(db);

    }

}
