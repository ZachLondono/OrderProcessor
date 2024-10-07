using Domain.Orders.Builders;
using Domain.ValueObjects;
using FluentAssertions;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;

namespace Domain.Tests.Unit.ProductPlanner.Cabinets;

public class DiagonalBaseCornerCabinetTests {

    [Fact]
    public void DiagonalBaseCornerCabinet_Should_NotHaveGarageMaterialWhenIsNotGarage() {

        var cabinet = new BaseDiagonalCornerCabinetBuilder()
            .WithIsGarage(false)
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

        // Act
        var products = cabinet.GetPPProducts();

        // Assert
        products.Should().HaveCount(1);
        products.First().MaterialType.Should().NotBe("Garage");

    }

    [Fact]
    public void DiagonalBaseCornerBaseCabinet_Should_HaveGarageMaterialWhenIsGarage() {

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

        // Act
        var products = cabinet.GetPPProducts();

        // Assert
        products.Should().HaveCount(1);
        products.First().MaterialType.Should().Be("Garage");

    }

}
