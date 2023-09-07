using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.Cabinets;

public class TallCabinetPersistenceTest : PersistenceTests {

    [Fact]
    public void InsertOrderWithTallCabinet() {
        var cabinet = new TallCabinetBuilder()
            .WithDoors(TallCabinetDoors.NoDoors())
            .WithInside(TallCabinetInside.Empty())
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public void DeleteOrderWithTallCabinet() {
        var cabinet = new TallCabinetBuilder()
            .WithDoors(TallCabinetDoors.NoDoors())
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        InsertAndDeleteOrderWithProduct(cabinet);
    }

    [Fact]
    public void InsertOrderWithGarageTallCabinet() {
        var cabinet = new TallCabinetBuilder()
            .WithIsGarage(true)
            .WithDoors(TallCabinetDoors.NoDoors())
            .WithInside(TallCabinetInside.Empty())
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();

        InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public void DeleteOrderWithGarageTallCabinet() {
        var cabinet = new TallCabinetBuilder()
            .WithIsGarage(true)
            .WithDoors(TallCabinetDoors.NoDoors())
            .WithInside(TallCabinetInside.Empty())
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();

        InsertAndDeleteOrderWithProduct(cabinet);
    }

    [Fact]
    public void InsertOrderWithTallCabinetWithProductionNotes() {
        var cabinet = new TallCabinetBuilder()
            .WithDoors(TallCabinetDoors.NoDoors())
            .WithInside(TallCabinetInside.Empty())
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .WithProductionNotes(new() { "A", "B", "C" })
            .Build();

        InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public void DeleteOrderWithTallCabinetWithProductionNotes() {
        var cabinet = new TallCabinetBuilder()
            .WithDoors(TallCabinetDoors.NoDoors())
            .WithInside(TallCabinetInside.Empty())
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .WithProductionNotes(new() { "A", "B", "C" })
            .Build();

        InsertAndDeleteOrderWithProduct(cabinet);
    }
}
