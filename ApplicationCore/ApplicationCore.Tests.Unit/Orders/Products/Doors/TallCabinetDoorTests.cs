using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using Domain.ValueObjects;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Products.Doors;

public class TallCabinetDoorTests {

    private readonly Func<MDFDoorBuilder> _doorBuilderFactory;
    private readonly MDFDoorOptions _mdfOptions;

    public TallCabinetDoorTests() {

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
        var cabinet = new TallCabinetBuilder()
                            .WithDoors(new(HingeSide.Left))
                            .WithToeType(ToeType.LegLevelers)
                            .WithWidth(Dimension.FromMillimeters(457))
                            .WithHeight(Dimension.FromMillimeters(2134))
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
        var cabinet = new TallCabinetBuilder()
                            .WithDoors(new(HingeSide.Left))
                            .WithToeType(ToeType.LegLevelers)
                            .WithWidth(Dimension.FromMillimeters(457))
                            .WithHeight(Dimension.FromMillimeters(2134))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .WithMDFDoorOptions(null)
                            .Build();


        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().BeEmpty();

    }

    [Theory]
    [InlineData(2134, HingeSide.Left, 1, 2029)]
    [InlineData(2134, HingeSide.Right, 1, 2029)]
    [InlineData(2134, HingeSide.NotApplicable, 2, 2029)]
    [InlineData(2222, HingeSide.Left, 1, 2117)]
    public void SingleSectionDoorHeightTests(double cabHeight, HingeSide hingeSide, int expectedDoorQty, double expectedCabinetHeight) {

        // Arrange
        var cabinet = new TallCabinetBuilder()
                            .WithDoors(new(hingeSide))
                            .WithToeType(ToeType.LegLevelers)
                            .WithWidth(Dimension.FromMillimeters(457))
                            .WithHeight(Dimension.FromMillimeters(cabHeight))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .WithQty(1)
                            .WithMDFDoorOptions(_mdfOptions)
                            .Build();


        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().HaveCount(1);
        doors.Sum(d => d.Qty).Should().Be(expectedDoorQty);
        doors.First().Height.Should().Be(Dimension.FromMillimeters(expectedCabinetHeight));

    }

    [Theory]
    [InlineData(457, HingeSide.Left, 1, 453)]
    [InlineData(457, HingeSide.Right, 1, 453)]
    [InlineData(762, HingeSide.NotApplicable, 2, 377.5)]
    [InlineData(500, HingeSide.Left, 1, 496)]
    public void SingleSectionDoorWidthTests(double cabWidth, HingeSide hingeSide, int expectedDoorQty, double expectedCabinetWidth) {

        // Arrange
        var cabinet = new TallCabinetBuilder()
                            .WithDoors(new(hingeSide))
                            .WithWidth(Dimension.FromMillimeters(cabWidth))
                            .WithHeight(Dimension.FromMillimeters(2134))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .WithQty(1)
                            .WithMDFDoorOptions(_mdfOptions)
                            .Build();


        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().HaveCount(1);
        doors.Sum(d => d.Qty).Should().Be(expectedDoorQty);
        doors.First().Width.Should().Be(Dimension.FromMillimeters(expectedCabinetWidth));

    }

    [Theory]
    [InlineData(2134, 1267, HingeSide.Left, 2, 759)]
    [InlineData(2134, 1267, HingeSide.Right, 2, 759)]
    [InlineData(2134, 1267, HingeSide.NotApplicable, 4, 759)]
    [InlineData(2222, 1267, HingeSide.Left, 2, 847)]
    public void TwoSectionDoorHeightTests(double cabHeight, double lowerDoorHeight, HingeSide hingeSide, int expectedDoorQty, double expectedCabinetHeight) {

        // Arrange
        var cabinet = new TallCabinetBuilder()
                            .WithDoors(new(Dimension.FromMillimeters(lowerDoorHeight), hingeSide))
                            .WithToeType(ToeType.LegLevelers)
                            .WithWidth(Dimension.FromMillimeters(457))
                            .WithHeight(Dimension.FromMillimeters(cabHeight))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .WithQty(1)
                            .WithMDFDoorOptions(_mdfOptions)
                            .Build();


        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().HaveCount(2);
        doors.Sum(d => d.Qty).Should().Be(expectedDoorQty);
        doors.First().Height.Should().Be(Dimension.FromMillimeters(expectedCabinetHeight));
        doors.Skip(1).First().Height.Should().Be(Dimension.FromMillimeters(lowerDoorHeight));

    }

}
