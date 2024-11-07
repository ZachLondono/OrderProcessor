using Domain.Orders.Builders;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.Cabinets;

public class BaseDiagonalCornerCabinetPersistenceTest : PersistenceTests {

    [Fact]
    public async Task InsertOrderWithBaseDiagonalCornerCabinet() {
        var cabinet = new BaseDiagonalCornerCabinetBuilder()
            .WithRightWidth(Dimension.FromInches(12))
            .WithRightDepth(Dimension.FromInches(12))
            .WithHingeSide(HingeSide.NotApplicable)
            .WithDoorQty(1)
            .WithAdjustableShelves(1)
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        await InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task DeleteOrderWithBaseDiagonalCornerCabinet() {
        var cabinet = new BaseDiagonalCornerCabinetBuilder()
            .WithRightWidth(Dimension.FromInches(12))
            .WithRightDepth(Dimension.FromInches(12))
            .WithHingeSide(HingeSide.NotApplicable)
            .WithDoorQty(1)
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        await InsertAndDeleteOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task InsertOrderWithGarageBaseDiagonalCornerCabinet() {
        var cabinet = new BaseDiagonalCornerCabinetBuilder()
            .WithIsGarage(true)
            .WithRightWidth(Dimension.FromInches(12))
            .WithRightDepth(Dimension.FromInches(12))
            .WithHingeSide(HingeSide.NotApplicable)
            .WithDoorQty(1)
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();

        await InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task DeleteOrderWithGarageBaseDiagonalCornerCabinet() {
        var cabinet = new BaseDiagonalCornerCabinetBuilder()
            .WithIsGarage(true)
            .WithRightWidth(Dimension.FromInches(12))
            .WithRightDepth(Dimension.FromInches(12))
            .WithHingeSide(HingeSide.NotApplicable)
            .WithDoorQty(1)
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();

        await InsertAndDeleteOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task InsertOrderWithBaseDiagonalCornerCabinetWithProductionNotes() {
        var cabinet = new BaseDiagonalCornerCabinetBuilder()
            .WithRightWidth(Dimension.FromInches(12))
            .WithRightDepth(Dimension.FromInches(12))
            .WithHingeSide(HingeSide.NotApplicable)
            .WithDoorQty(1)
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .WithProductionNotes(new() { "A", "B", "C" })
            .Build();

        await InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public async Task DeleteOrderWithGarageBaseDiagonalCornerCabinetWithProductionNotes() {
        var cabinet = new BaseDiagonalCornerCabinetBuilder()
            .WithRightWidth(Dimension.FromInches(12))
            .WithRightDepth(Dimension.FromInches(12))
            .WithHingeSide(HingeSide.NotApplicable)
            .WithDoorQty(1)
            .WithToeType(ToeType.NoToe)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .WithProductionNotes(new() { "A", "B", "C" })
            .Build();

        await InsertAndDeleteOrderWithProduct(cabinet);
    }

}