using Domain.Orders.Builders;
using Domain.ValueObjects;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.Cabinets;

public class BasePieCutCornerCabinetPersistenceTest : PersistenceTests {

    [Fact]
    public async Task InsertOrderWithBasePieCutCornerCabinet() {
        var cabinet = new BasePieCutCornerCabinetBuilder()
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        await InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task DeleteOrderWithBasePieCutCornerCabinet() {
        var cabinet = new BasePieCutCornerCabinetBuilder()
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        await InsertAndDeleteOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task InsertOrderWithBasePieCutCornerCabinetWithProductionNotes() {
        var cabinet = new BasePieCutCornerCabinetBuilder()
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .WithProductionNotes(new() { "A", "B", "C" })
            .Build();

        await InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task DeleteOrderWithBasePieCutCornerCabinetWithProductionNotes() {
        var cabinet = new BasePieCutCornerCabinetBuilder()
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .WithProductionNotes(new() { "A", "B", "C" })
            .Build();

        await InsertAndDeleteOrderWithProduct(cabinet);
    }

}
