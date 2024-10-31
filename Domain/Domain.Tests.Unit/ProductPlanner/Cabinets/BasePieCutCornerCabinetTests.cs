using Domain.Orders.Builders;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Unit.ProductPlanner.Cabinets;

/*

BlindBaseCabinet
BlindWallCabinet
SinkCabinet
TrashCabinet
WallDiagonalCornerCabinet
WallPieCutCornerCabinet
*/


public class BasePieCutCornerCabinetTests {

    [Fact]
    public void DoorType_ShouldBeSlab_WhenSlabDoorMaterialIsNotNullAndMDFOptionsIsNull() {

        // Arrange
        var cabinet = new BasePieCutCornerCabinetBuilder()
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
        var cabinet = new BasePieCutCornerCabinetBuilder()
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
        var cabinet = new BasePieCutCornerCabinetBuilder()
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
