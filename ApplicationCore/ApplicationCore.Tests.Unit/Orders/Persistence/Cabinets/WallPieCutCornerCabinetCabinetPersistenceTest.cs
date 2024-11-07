using Domain.Orders.Builders;
using Domain.ValueObjects;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.Cabinets;

public class WallPieCutCornerCabinetCabinetPersistenceTest : PersistenceTests {

    [Fact]
    public async Task InsertOrderWithWallPieCutCornerCabinet() {
        var cabinet = new WallPieCutCornerCabinetBuilder()
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        await InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task DeleteOrderWithWallPieCutCabinet() {
        var cabinet = new WallPieCutCornerCabinetBuilder()
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        await InsertAndDeleteOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task InsertOrderWithWallPieCutCabinetWithProductionNotes() {
        var cabinet = new WallPieCutCornerCabinetBuilder()
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .WithProductionNotes(new() { "A", "B", "C" })
            .Build();

        await InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task DeleteOrderWithWallPieCutCabinetWithProductionNotes() {
        var cabinet = new WallPieCutCornerCabinetBuilder()
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .WithProductionNotes(new() { "A", "B", "C" })
            .Build();

        await InsertAndDeleteOrderWithProduct(cabinet);
    }

}
