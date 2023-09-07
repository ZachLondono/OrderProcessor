using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.Cabinets;

public class BaseCabinetPersistenceTest : PersistenceTests {

    [Fact]
    public void InsertOrderWithBaseCabinet() {
        var cabinet = new BaseCabinetBuilder()
            .WithDoors(BaseCabinetDoors.NoDoors())
            .WithInside(BaseCabinetInside.Empty())
            .WithDrawers(HorizontalDrawerBank.None())
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public void DeleteOrderWithBaseCabinet() {
        var cabinet = new BaseCabinetBuilder()
            .WithDoors(BaseCabinetDoors.NoDoors())
            .WithInside(BaseCabinetInside.Empty())
            .WithDrawers(HorizontalDrawerBank.None())
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        InsertAndDeleteOrderWithProduct(cabinet);
    }

    [Fact]
    public void InsertOrderWithGarageBaseCabinet() {
        var cabinet = new BaseCabinetBuilder()
            .WithIsGarage(true)
            .WithDoors(BaseCabinetDoors.NoDoors())
            .WithInside(BaseCabinetInside.Empty())
            .WithDrawers(HorizontalDrawerBank.None())
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();

        InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public void DeleteOrderWithGarageBaseCabinet() {
        var cabinet = new BaseCabinetBuilder()
            .WithIsGarage(true)
            .WithDoors(BaseCabinetDoors.NoDoors())
            .WithInside(BaseCabinetInside.Empty())
            .WithDrawers(HorizontalDrawerBank.None())
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();

        InsertAndDeleteOrderWithProduct(cabinet);
    }

}
