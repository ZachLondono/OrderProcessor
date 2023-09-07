using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Shared.Domain;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.ProductPlanner.Cabinets;

public class DrawerBaseCabinetTests {

    [Fact]
    public void DrawerBaseCabinet_Should_NotHaveGarageMaterialWhenIsNotGarage() {

        var cabinet = new DrawerBaseCabinetBuilder()
            .WithIsGarage(false)
            .WithDrawers(VerticalDrawerBank.None())
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
    public void DrawerBaseCabinet_Should_HaveGarageMaterialWhenIsGarage() {

        var cabinet = new DrawerBaseCabinetBuilder()
            .WithIsGarage(true)
            .WithDrawers(VerticalDrawerBank.None())
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
