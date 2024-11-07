using Domain.Orders.Builders;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.Cabinets;

public class WallCabinetPersistenceTest : PersistenceTests {

    [Fact]
    public async Task InsertOrderWithWallCabinet() {
        var cabinet = new WallCabinetBuilder()
            .WithDoors(WallCabinetDoors.NoDoors())
            .WithInside(WallCabinetInside.Empty())
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        await InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task DeleteOrderWithWallCabinet() {
        var cabinet = new WallCabinetBuilder()
            .WithDoors(WallCabinetDoors.NoDoors())
            .WithInside(WallCabinetInside.Empty())
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        await InsertAndDeleteOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task InsertOrderWithGarageWallCabinet() {
        var cabinet = new WallCabinetBuilder()
            .WithIsGarage(true)
            .WithDoors(WallCabinetDoors.NoDoors())
            .WithInside(WallCabinetInside.Empty())
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        await InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task DeleteOrderWithGarageWallCabinet() {
        var cabinet = new WallCabinetBuilder()
            .WithIsGarage(true)
            .WithDoors(WallCabinetDoors.NoDoors())
            .WithInside(WallCabinetInside.Empty())
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();

        await InsertAndDeleteOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task InsertOrderWithWallCabinetWithProductionNotes() {
        var cabinet = new WallCabinetBuilder()
            .WithDoors(WallCabinetDoors.NoDoors())
            .WithInside(WallCabinetInside.Empty())
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .WithProductionNotes(new() { "A", "B", "C" })
            .Build();
        await InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task DeleteOrderWithWallCabinetWithProductionNotes() {
        var cabinet = new WallCabinetBuilder()
            .WithDoors(WallCabinetDoors.NoDoors())
            .WithInside(WallCabinetInside.Empty())
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .WithProductionNotes(new() { "A", "B", "C" })
            .Build();

        await InsertAndDeleteOrderWithProduct(cabinet);
    }

}