using Domain.Orders.Builders;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Unit.Orders.Products.CabinetDoors;

public class BlindWallCabinetDoorTests {

    private readonly Func<MDFDoorBuilder> _doorBuilderFactory;
    private readonly MDFDoorOptions _mdfOptions;

    public BlindWallCabinetDoorTests() {

        var doorConfiguration = new MDFDoorConfiguration() {
            TopRail = Dimension.Zero,
            BottomRail = Dimension.Zero,
            LeftStile = Dimension.Zero,
            RightStile = Dimension.Zero,
            EdgeDetail = "",
            FramingBead = "",
            Material = "",
            PanelDetail = "",
            Thickness = Dimension.Zero
        };

        _doorBuilderFactory = () => new(doorConfiguration);

        _mdfOptions = new("MDF", Dimension.Zero, "Shaker", "Eased", "Flat", Dimension.Zero, null);

    }

    [Fact]
    public void GetDoors_ShouldReturnCorrectQty_WhenCabinetQtyIsGreatorThan1() {

        int cabinetQty = 2;
        int doorQty = 2;

        // Arrange
        var cabinet = new BlindWallCabinetBuilder()
                            .WithDoors(new() { Quantity = doorQty })
                            .WithBlindWidth(Dimension.FromMillimeters(339))
                            .WithWidth(Dimension.FromMillimeters(685))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .WithQty(cabinetQty)
                            .WithDoorConfiguration(_mdfOptions)
                            .Build();


        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Sum(d => d.Qty).Should().Be(cabinetQty * doorQty);

    }

    [Fact]
    public void GetDoors_ShouldReturnEmpty_WhenMDFDoorOptionsByOthers() {

        // Arrange
        var cabinet = new BlindWallCabinetBuilder()
                            .WithDoors(new() { Quantity = 1 })
                            .WithBlindWidth(Dimension.FromMillimeters(339))
                            .WithWidth(Dimension.FromMillimeters(685))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .WithDoorConfiguration(new DoorsByOthers())
                            .Build();


        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().BeEmpty();

    }

    [Fact]
    public void GetDoors_ShouldReturnEmpty_WhenMDFDoorOptionsSlab() {

        // Arrange
        var cabinet = new BlindWallCabinetBuilder()
                            .WithDoors(new() { Quantity = 1 })
                            .WithBlindWidth(Dimension.FromMillimeters(339))
                            .WithWidth(Dimension.FromMillimeters(685))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .WithDoorConfiguration(new CabinetSlabDoorMaterial("Finish", CabinetMaterialFinishType.Melamine, CabinetMaterialCore.ParticleBoard))
                            .Build();


        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().BeEmpty();

    }

    [Theory]
    [InlineData(685, 1, 339, 342.5)]
    [InlineData(993, 2, 339, 323.75)]
    public void DoorWidthTests(double cabWidth, int doorQty, double blindWidth, double expectedDoorWidth) {

        // Arrange
        var cabinet = new BlindWallCabinetBuilder()
                            .WithDoors(new() { Quantity = doorQty })
                            .WithBlindWidth(Dimension.FromMillimeters(blindWidth))
                            .WithWidth(Dimension.FromMillimeters(cabWidth))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .WithDoorConfiguration(_mdfOptions)
                            .Build();


        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().HaveCount(1);
        doors.First().Width.Should().Be(Dimension.FromMillimeters(expectedDoorWidth));

    }

    [Theory]
    [InlineData(914, 911)]
    [InlineData(876, 873)]
    public void DoorHeightTests(double cabHeight, double expectedDoorHeight) {

        // Arrange
        var cabinet = new BlindWallCabinetBuilder()
                            .WithDoors(new() { Quantity = 1 })
                            .WithWidth(Dimension.FromMillimeters(456))
                            .WithHeight(Dimension.FromMillimeters(cabHeight))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .WithDoorConfiguration(_mdfOptions)
                            .Build();


        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().HaveCount(1);
        doors.First().Height.Should().Be(Dimension.FromMillimeters(expectedDoorHeight));

    }

    [Theory]
    [InlineData(914, 19, 930)]
    [InlineData(876, 19, 892)]
    public void DoorHeightTests_WithExtension(double cabHeight, double extendDown, double expectedDoorHeight) {

        // Arrange
        var cabinet = new BlindWallCabinetBuilder()
                            .WithDoors(new() { Quantity = 1 })
                            .WithExtendedDoor(Dimension.FromMillimeters(extendDown))
                            .WithWidth(Dimension.FromMillimeters(456))
                            .WithHeight(Dimension.FromMillimeters(cabHeight))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .WithDoorConfiguration(_mdfOptions)
                            .Build();


        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().HaveCount(1);
        doors.First().Height.Should().Be(Dimension.FromMillimeters(expectedDoorHeight));

    }


}