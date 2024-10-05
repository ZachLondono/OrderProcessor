using Domain.Orders.Builders;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Unit.Orders.Products.CabinetDoors;

public class WallPieCutCornerCabinetDoorTests {

    private readonly Func<MDFDoorBuilder> _doorBuilderFactory;
    private readonly MDFDoorOptions _mdfOptions;

    public WallPieCutCornerCabinetDoorTests() {

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
        var cabinet = new WallPieCutCornerCabinetBuilder()
                            .WithRightWidth(Dimension.FromMillimeters(610))
                            .WithRightDepth(Dimension.FromMillimeters(305))
                            .WithWidth(Dimension.FromMillimeters(610))
                            .WithDepth(Dimension.FromMillimeters(305))
                            .WithHeight(Dimension.FromMillimeters(914))
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
        var cabinet = new WallPieCutCornerCabinetBuilder()
                            .WithRightWidth(Dimension.FromMillimeters(610))
                            .WithRightDepth(Dimension.FromMillimeters(305))
                            .WithWidth(Dimension.FromMillimeters(610))
                            .WithDepth(Dimension.FromMillimeters(305))
                            .WithHeight(Dimension.FromMillimeters(914))
                            .WithMDFDoorOptions(null)
                            .Build();


        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().BeEmpty();

    }

    [Theory]
    [InlineData(610, 610, 305, 305, 281, 281)]
    [InlineData(610, 710, 400, 305, 286, 281)]
    public void DoorWidthTest(double cabWidth, double rightWidth, double cabDepth, double rightDepth, double expectedDoorWidthA, double expectedDoorWidthB) {

        // Arrange
        var cabinet = new WallPieCutCornerCabinetBuilder()
                            .WithRightWidth(Dimension.FromMillimeters(rightWidth))
                            .WithRightDepth(Dimension.FromMillimeters(rightDepth))
                            .WithWidth(Dimension.FromMillimeters(cabWidth))
                            .WithDepth(Dimension.FromMillimeters(cabDepth))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithMDFDoorOptions(_mdfOptions)
                            .Build();


        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().HaveCount(2);
        doors.Should().Contain(d => d.Width == Dimension.FromMillimeters(expectedDoorWidthA));
        doors.Should().Contain(d => d.Width == Dimension.FromMillimeters(expectedDoorWidthB));

    }

    [Theory]
    [InlineData(914, 911)]
    public void DoorHeightTest(double cabHeight, double expectedDoorHeight) {

        // Arrange
        var cabinet = new WallPieCutCornerCabinetBuilder()
                            .WithRightWidth(Dimension.FromMillimeters(610))
                            .WithRightDepth(Dimension.FromMillimeters(305))
                            .WithWidth(Dimension.FromMillimeters(610))
                            .WithDepth(Dimension.FromMillimeters(305))
                            .WithHeight(Dimension.FromMillimeters(cabHeight))
                            .WithMDFDoorOptions(_mdfOptions)
                            .Build();


        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.First().Height.Should().Be(Dimension.FromMillimeters(expectedDoorHeight));

    }

    [Theory]
    [InlineData(914, 19, 930)]
    public void DoorHeightTest_WithExtension(double cabHeight, double extendDown, double expectedDoorHeight) {

        // Arrange
        var cabinet = new WallPieCutCornerCabinetBuilder()
                            .WithExtendedDoor(Dimension.FromMillimeters(extendDown))
                            .WithRightWidth(Dimension.FromMillimeters(610))
                            .WithRightDepth(Dimension.FromMillimeters(305))
                            .WithWidth(Dimension.FromMillimeters(610))
                            .WithDepth(Dimension.FromMillimeters(305))
                            .WithHeight(Dimension.FromMillimeters(cabHeight))
                            .WithMDFDoorOptions(_mdfOptions)
                            .Build();


        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.First().Height.Should().Be(Dimension.FromMillimeters(expectedDoorHeight));

    }


}