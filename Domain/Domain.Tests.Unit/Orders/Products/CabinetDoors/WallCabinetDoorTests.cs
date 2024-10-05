using Domain.Orders.Builders;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Unit.Orders.Products.CabinetDoors;

public class WallCabinetDoorTests {

    private readonly Func<MDFDoorBuilder> _doorBuilderFactory;
    private readonly MDFDoorOptions _mdfOptions;

    public WallCabinetDoorTests() {

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
        int doorQty = 1;

        // Arrange
        var cabinet = new WallCabinetBuilder()
                            .WithDoors(new() { Quantity = doorQty })
                            .WithWidth(Dimension.FromMillimeters(456))
                            .WithHeight(Dimension.FromMillimeters(914))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .WithQty(cabinetQty)
                            .WithMDFDoorOptions(_mdfOptions)
                            .Build();


        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Sum(d => d.Qty).Should().Be(cabinetQty * doorQty);

    }

    [Fact]
    public void GetDoors_ShouldReturnEmpty_WhenMDFDoorOptionsIsNull() {

        // Arrange
        var cabinet = new WallCabinetBuilder()
                            .WithDoors(new() { Quantity = 1 })
                            .WithWidth(Dimension.FromMillimeters(456))
                            .WithHeight(Dimension.FromMillimeters(914))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .WithMDFDoorOptions(null)
                            .Build();


        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().BeEmpty();

    }

    [Theory]
    [InlineData(914, 911)]
    [InlineData(756, 753)]
    public void DoorHeightTests(double cabHeight, double expectedDoorHeight) {

        // Arrange
        var cabinet = new WallCabinetBuilder()
                            .WithDoors(new() { Quantity = 1 })
                            .WithWidth(Dimension.FromMillimeters(456))
                            .WithHeight(Dimension.FromMillimeters(cabHeight))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .WithMDFDoorOptions(_mdfOptions)
                            .Build();



        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().HaveCount(1);
        doors.First().Height.Should().Be(Dimension.FromMillimeters(expectedDoorHeight));

    }

    [Theory]
    [InlineData(406, 1, 402)]
    [InlineData(610, 2, 301.5)]
    public void DoorWidthTests(double cabWidth, int doorQty, double expectedDoorWidth) {

        // Arrange
        var cabinet = new WallCabinetBuilder()
                            .WithDoors(new() { Quantity = doorQty })
                            .WithWidth(Dimension.FromMillimeters(cabWidth))
                            .WithHeight(Dimension.FromMillimeters(914))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .WithMDFDoorOptions(_mdfOptions)
                            .Build();



        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().HaveCount(1);
        doors.First().Width.Should().Be(Dimension.FromMillimeters(expectedDoorWidth));

    }

    [Theory]
    [InlineData(914, 19, 930)]
    [InlineData(756, 19, 772)]
    public void DoorHeightTests_WithExtension(double cabHeight, double extendDown, double expectedDoorHeight) {

        // Arrange
        var cabinet = new WallCabinetBuilder()
                            .WithDoors(new() { Quantity = 1, ExtendDown = Dimension.FromMillimeters(extendDown) })
                            .WithWidth(Dimension.FromMillimeters(456))
                            .WithHeight(Dimension.FromMillimeters(cabHeight))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .WithMDFDoorOptions(_mdfOptions)
                            .Build();



        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().HaveCount(1);
        doors.First().Height.Should().Be(Dimension.FromMillimeters(expectedDoorHeight));

    }

}
