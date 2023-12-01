using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Products.Doors;

public class WallDiagonalCornerCabinetDoorTests {

    private readonly Func<MDFDoorBuilder> _doorBuilderFactory;
    private readonly MDFDoorOptions _mdfOptions;

    public WallDiagonalCornerCabinetDoorTests() {

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
        var cabinet = new WallDiagonalCornerCabinetBuilder()
                            .WithDoorQty(doorQty)
                            .WithRightWidth(Dimension.FromMillimeters(610))
                            .WithRightDepth(Dimension.FromMillimeters(305))
                            .WithWidth(Dimension.FromMillimeters(610))
                            .WithDepth(Dimension.FromMillimeters(305))
                            .WithHeight(Dimension.FromMillimeters(876))
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
        var cabinet = new WallDiagonalCornerCabinetBuilder()
                            .WithDoorQty(1)
                            .WithRightWidth(Dimension.FromMillimeters(610))
                            .WithRightDepth(Dimension.FromMillimeters(305))
                            .WithWidth(Dimension.FromMillimeters(610))
                            .WithDepth(Dimension.FromMillimeters(305))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithMDFDoorOptions(null)
                            .Build();


        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().BeEmpty();

    }

    [Theory]
    [InlineData(610, 610, 305, 305, 1, 398.5)]
    [InlineData(610, 610, 305, 305, 2, 197.5)]
    [InlineData(610, 710, 400, 305, 2, 199.5)]
    public void DoorWidthTest(double cabWidth, double rightWidth, double cabDepth, double rightDepth, int doorQty, double expectedDoorWidth) {

        // Arrange
        var cabinet = new WallDiagonalCornerCabinetBuilder()
                            .WithDoorQty(doorQty)
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
        doors.Should().HaveCount(1);
        doors.First().Width.AsMillimeters().Should().BeInRange(expectedDoorWidth - 0.5, expectedDoorWidth + 0.5);

    }

    [Theory]
    [InlineData(914, 911)]
    public void DoorHeightTest(double cabHeight, double expectedDoorHeight) {

        // Arrange
        var cabinet = new WallDiagonalCornerCabinetBuilder()
                            .WithDoorQty(1)
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
        var cabinet = new WallDiagonalCornerCabinetBuilder()
                            .WithDoorQty(1)
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