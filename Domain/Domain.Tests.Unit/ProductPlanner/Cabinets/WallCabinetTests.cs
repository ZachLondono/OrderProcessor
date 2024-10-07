using Domain.Orders.Builders;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Unit.ProductPlanner.Cabinets;

public class WallCabinetTests {

    [Fact]
    public void WallCabinet_Should_NotHaveGarageMaterialWhenIsNotGarage() {

        var cabinet = new WallCabinetBuilder()
            .WithIsGarage(false)
            .WithDoors(WallCabinetDoors.NoDoors())
            .WithInside(WallCabinetInside.Empty())
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
    public void WallCabinet_Should_HaveGarageMaterialWhenIsGarage() {

        var cabinet = new WallCabinetBuilder()
            .WithIsGarage(true)
            .WithDoors(WallCabinetDoors.NoDoors())
            .WithInside(WallCabinetInside.Empty())
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
