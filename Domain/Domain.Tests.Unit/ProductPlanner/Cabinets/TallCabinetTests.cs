using Domain.Orders.Builders;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Unit.ProductPlanner.Cabinets;

public class TallCabinetTests {

    [Fact]
    public void TallCabinet_Should_NotHaveGarageMaterialWhenIsNotGarage() {

        var cabinet = new TallCabinetBuilder()
            .WithIsGarage(false)
            .WithDoors(TallCabinetDoors.NoDoors())
            .WithInside(TallCabinetInside.Empty())
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
    public void TallCabinet_Should_HaveGarageMaterialWhenIsGarage() {

        var cabinet = new TallCabinetBuilder()
            .WithIsGarage(true)
            .WithDoors(TallCabinetDoors.NoDoors())
            .WithInside(TallCabinetInside.Empty())
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
