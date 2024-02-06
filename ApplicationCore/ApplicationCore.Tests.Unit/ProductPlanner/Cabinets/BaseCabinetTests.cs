using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Shared.Domain;
using Domain.ValueObjects;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.ProductPlanner.Cabinets;

public class BaseCabinetTests {

    [Fact]
    public void BaseCabinet_Should_NotHaveGarageMaterialWhenIsNotGarage() {

        // Arrange
        var cabinet = new BaseCabinetBuilder()
            .WithIsGarage(false)
            .WithDoors(BaseCabinetDoors.NoDoors())
            .WithInside(BaseCabinetInside.Empty())
            .WithDrawers(HorizontalDrawerBank.None())
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
    public void BaseCabinet_Should_HaveGarageMaterialWhenIsGarage() {

        // Arrange
        var cabinet = new BaseCabinetBuilder()
            .WithIsGarage(true)
            .WithDoors(BaseCabinetDoors.NoDoors())
            .WithInside(BaseCabinetInside.Empty())
            .WithDrawers(HorizontalDrawerBank.None())
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
