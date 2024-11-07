using Domain.Orders.Builders;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Unit.ProductPlanner.Cabinets;

public class WallCabinetTests {

    [Fact]
    public void WallCabinet_ShouldNotHaveGarageMaterial_WhenIsNotGarage() {

        // Arrange
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
    public void WallCabinet_ShouldHaveGarageMaterial_WhenIsGarage() {

        // Arrange
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

    [Fact]
    public void DoorType_ShouldBeSlab_WhenDoorConfigurationIsSlab() {

        // Arrange
        var cabinet = new WallCabinetBuilder()
                            .WithDoorConfiguration(new CabinetSlabDoorMaterial("", Domain.Orders.Enums.CabinetMaterialFinishType.Melamine, Domain.Orders.Enums.CabinetMaterialCore.ParticleBoard, null))
                            .Build();

        // Act
        var products = cabinet.GetPPProducts();

        // Assert
        products.Should().AllBeEquivalentTo(new {
            DoorType = Cabinet.SLAB_DOOR_TYPE
        }, o => o.ExcludingMissingMembers());

    }

    [Fact]
    public void DoorType_ShouldBeByOut_WhenDoorConfigurationIsMDF() {

        // Arrange
        var cabinet = new WallCabinetBuilder()
                            .WithDoorConfiguration(new MDFDoorOptions("", Dimension.Zero, "", "", "", Dimension.Zero, null))
                            .Build();

        // Act
        var products = cabinet.GetPPProducts();

        // Assert
        products.Should().AllBeEquivalentTo(new {
            DoorType = Cabinet.BUYOUT_DOOR_TYPE
        }, o => o.ExcludingMissingMembers());

    }

    [Fact]
    public void DoorType_ShouldBeByOut_WhenDoorConfigurationIsByOthers() {

        // Arrange
        var cabinet = new WallCabinetBuilder()
                            .WithDoorConfiguration(new DoorsByOthers())
                            .Build();

        // Act
        var products = cabinet.GetPPProducts();

        // Assert
        products.Should().AllBeEquivalentTo(new {
            DoorType = Cabinet.BUYOUT_DOOR_TYPE
        }, o => o.ExcludingMissingMembers());

    }

}
