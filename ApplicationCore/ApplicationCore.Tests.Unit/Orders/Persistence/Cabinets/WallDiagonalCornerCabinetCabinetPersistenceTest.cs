using Domain.Orders.Builders;
using Domain.Orders.Enums;
using Domain.ValueObjects;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.Cabinets;

public class WallDiagonalCornerCabinetCabinetPersistenceTest : PersistenceTests {

    [Fact]
    public async Task InsertOrderWithWallDiagonalCornerCabinet() {
        var cabinet = new WallDiagonalCornerCabinetBuilder()
            .WithRightDepth(Dimension.FromInches(12))
            .WithRightWidth(Dimension.FromInches(12))
            .WithDoorQty(1)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        await InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task DeleteOrderWithWallDiagonalCabinet() {
        var cabinet = new WallDiagonalCornerCabinetBuilder()
            .WithRightDepth(Dimension.FromInches(12))
            .WithRightWidth(Dimension.FromInches(12))
            .WithDoorQty(1)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        await InsertAndDeleteOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task InsertOrderWithWallBaseDiagonalCornerCabinet() {
        var cabinet = new WallDiagonalCornerCabinetBuilder()
            .WithIsGarage(true)
            .WithRightWidth(Dimension.FromInches(12))
            .WithRightDepth(Dimension.FromInches(12))
            .WithHingeSide(HingeSide.NotApplicable)
            .WithDoorQty(1)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();

        await InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task DeleteOrderWithGarageWallDiagonalCornerCabinet() {
        var cabinet = new WallDiagonalCornerCabinetBuilder()
            .WithIsGarage(true)
            .WithRightWidth(Dimension.FromInches(12))
            .WithRightDepth(Dimension.FromInches(12))
            .WithHingeSide(HingeSide.NotApplicable)
            .WithDoorQty(1)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();

        await InsertAndDeleteOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task InsertOrderWithWallDiagonalCabinetWithProductionNotes() {
        var cabinet = new WallDiagonalCornerCabinetBuilder()
            .WithRightDepth(Dimension.FromInches(12))
            .WithRightWidth(Dimension.FromInches(12))
            .WithDoorQty(1)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .WithProductionNotes(new() { "A", "B", "C" })
            .Build();

        await InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task DeleteOrderWithWallDiagonalCabinetWithProductionNotes() {
        var cabinet = new WallDiagonalCornerCabinetBuilder()
            .WithRightDepth(Dimension.FromInches(12))
            .WithRightWidth(Dimension.FromInches(12))
            .WithDoorQty(1)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .WithProductionNotes(new() { "A", "B", "C" })
            .Build();

        await InsertAndDeleteOrderWithProduct(cabinet);
    }

}
