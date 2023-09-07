using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Tests.Unit.Orders.Persistence.Cabinets;

public class BaseDiagonalCornerCabinetPersistenceTest : PersistenceTests {

    [Fact]
    public void InsertOrderWithBaseDiagonalCornerCabinet() {
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
        InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public void DeleteOrderWithBaseDiagonalCornerCabinet() {
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
        InsertAndDeleteOrderWithProduct(cabinet);
    }

    [Fact]
    public void InsertOrderWithGarageBaseDiagonalCornerCabinet() {
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

        InsertAndQueryOrderWithProduct(cabinet);
    }

    [Fact]
    public void DeleteOrderWithGarageBaseDiagonalCornerCabinet() {
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

        InsertAndDeleteOrderWithProduct(cabinet);
    }

}