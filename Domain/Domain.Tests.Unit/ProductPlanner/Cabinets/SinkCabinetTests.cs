using Domain.Orders.Builders;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Unit.ProductPlanner.Cabinets;

public class SinkCabinetTests {

    [Fact]
    public void DoorType_ShouldBeSlab_WhenDoorConfigurationIsSlab() {

        // Arrange
        var cabinet = new SinkCabinetBuilder()
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
        var cabinet = new SinkCabinetBuilder()
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
        var cabinet = new SinkCabinetBuilder()
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
