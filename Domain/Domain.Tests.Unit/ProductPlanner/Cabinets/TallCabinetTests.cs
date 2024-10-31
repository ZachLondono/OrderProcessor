using Domain.Orders.Builders;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Unit.ProductPlanner.Cabinets;

public class TallCabinetTests {

    [Fact]
    public void TallCabinet_ShouldNotHaveGarageMaterial_WhenIsNotGarage() {

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
    public void TallCabinet_ShouldHaveGarageMaterial_WhenIsGarage() {

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

    [Fact]
    public void DoorType_ShouldBeSlab_WhenSlabDoorMaterialIsNotNullAndMDFOptionsIsNull() {

        // Arrange
        var cabinet = new TallCabinetBuilder()
                            .WithMDFDoorOptions(null)
                            .WithSlabDoorMaterial(new("", Domain.Orders.Enums.CabinetMaterialFinishType.Melamine, Domain.Orders.Enums.CabinetMaterialCore.ParticleBoard, null))
                            .Build();

        // Act
        var products = cabinet.GetPPProducts();

        // Assert
        products.Should().AllBeEquivalentTo(new {
            DoorType = Cabinet.SLAB_DOOR_TYPE
        }, o => o.ExcludingMissingMembers());

    }

    [Fact]
    public void DoorType_ShouldBeByOut_WhenSlabDoorMaterialIsNullAndMDFOptionsIsNotNull() {

        // Arrange
        var cabinet = new TallCabinetBuilder()
                            .WithMDFDoorOptions(new("", Dimension.Zero, "", "", "", Dimension.Zero, null))
                            .WithSlabDoorMaterial(null)
                            .Build();

        // Act
        var products = cabinet.GetPPProducts();

        // Assert
        products.Should().AllBeEquivalentTo(new {
            DoorType = Cabinet.BUYOUT_DOOR_TYPE
        }, o => o.ExcludingMissingMembers());

    }

    [Fact]
    public void DoorType_ShouldBeByOut_WhenSlabDoorMaterialIsNullAndMDFOptionsIsNull() {

        // Arrange
        var cabinet = new TallCabinetBuilder()
                            .WithMDFDoorOptions(null)
                            .WithSlabDoorMaterial(null)
                            .Build();

        // Act
        var products = cabinet.GetPPProducts();

        // Assert
        products.Should().AllBeEquivalentTo(new {
            DoorType = Cabinet.BUYOUT_DOOR_TYPE
        }, o => o.ExcludingMissingMembers());

    }

}