using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.Cabinets;

public class WallCabinetPersistenceTest : PersistenceTests {

    [Fact]
    public void InsertOrderWithWallCabinet() {
        var cabinet = new WallCabinetBuilder()
            .WithDoors(WallCabinetDoors.NoDoors())
            .WithInside(WallCabinetInside.Empty())
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public void DeleteOrderWithWallCabinet() {
        var cabinet = new WallCabinetBuilder()
            .WithDoors(WallCabinetDoors.NoDoors())
            .WithInside(WallCabinetInside.Empty())
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        InsertAndDeleteOrderWithProduct(cabinet);
    }

    [Fact]
    public void InsertOrderWithGarageWallCabinet() {
        var cabinet = new WallCabinetBuilder()
            .WithIsGarage(true)
            .WithDoors(WallCabinetDoors.NoDoors())
            .WithInside(WallCabinetInside.Empty())
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public void DeleteOrderWithGarageWallCabinet() {
        var cabinet = new WallCabinetBuilder()
            .WithIsGarage(true)
            .WithDoors(WallCabinetDoors.NoDoors())
            .WithInside(WallCabinetInside.Empty())
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();

        InsertAndDeleteOrderWithProduct(cabinet);
    }

}