using Domain.Orders.Builders;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Unit.ProductPlanner.Cabinets;

public class BaseCabinetTests {

    [Fact]
    public void BaseCabinet_ShouldNotHaveGarageMaterial_WhenIsNotGarage() {

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
    public void BaseCabinet_ShouldHaveGarageMaterial_WhenIsGarage() {

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

    [Fact]
    public void DoorType_ShouldBeSlab_WhenDoorConfigurationIsSlab() {

        // Arrange
        var cabinet = new BaseCabinetBuilder()
                            .WithDoorConfiguration(new CabinetSlabDoorMaterial("", CabinetMaterialFinishType.Melamine, CabinetMaterialCore.ParticleBoard, null))
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
        var cabinet = new BaseCabinetBuilder()
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
    public void DoorType_ShouldBeByOut_WhenDoorConfigurationIsDoorsByOthers() {

        // Arrange
        var cabinet = new BaseCabinetBuilder()
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
