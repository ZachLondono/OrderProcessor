using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.Cabinets;

public class WallDiagonalCornerCabinetCabinetPersistenceTest : PersistenceTests {

    [Fact]
    public void InsertOrderWithWallDiagonalCornerCabinet() {
        var cabinet = new WallDiagonalCornerCabinetBuilder()
            .WithRightDepth(Dimension.FromInches(12))
            .WithRightWidth(Dimension.FromInches(12))
            .WithDoorQty(1)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public void DeleteOrderWithWallDiagonalCabinet() {
        var cabinet = new WallDiagonalCornerCabinetBuilder()
            .WithRightDepth(Dimension.FromInches(12))
            .WithRightWidth(Dimension.FromInches(12))
            .WithDoorQty(1)
            .WithWidth(Dimension.FromInches(25))
            .WithDepth(Dimension.FromInches(25))
            .WithHeight(Dimension.FromInches(25))
            .WithQty(1)
            .Build();
        InsertAndDeleteOrderWithProduct(cabinet);
    }

    [Fact]
    public void InsertOrderWithWallBaseDiagonalCornerCabinet() {
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

        InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public void DeleteOrderWithGarageWallDiagonalCornerCabinet() {
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

        InsertAndDeleteOrderWithProduct(cabinet);
    }

    [Fact]
    public void InsertOrderWithWallDiagonalCabinetWithProductionNotes() {
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

        InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public void DeleteOrderWithWallDiagonalCabinetWithProductionNotes() {
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

        InsertAndDeleteOrderWithProduct(cabinet);
    }

}
