using Domain.Orders.Builders;
using Domain.ValueObjects;
using FluentAssertions;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products.Cabinets;

namespace Domain.Tests.Unit.ProductPlanner.Cabinets;

public class DiagonalBaseCornerCabinetTests {

    [Fact]
    public void DiagonalBaseCornerCabinet_ShouldNotHaveGarageMaterial_WhenIsNotGarage() {

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
    public void DiagonalBaseCornerBaseCabinet_ShouldHaveGarageMaterial_WhenIsGarage() {

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

    [Fact]
    public void DoorType_ShouldBeSlab_WhenDoorConfigurationIsSlab() {

        // Arrange
        var cabinet = new BaseDiagonalCornerCabinetBuilder()
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
        var cabinet = new BaseDiagonalCornerCabinetBuilder()
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
        var cabinet = new BaseDiagonalCornerCabinetBuilder()
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
