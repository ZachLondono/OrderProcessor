using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Products;

public class BlindWallCabinetDoorTests {

    private readonly Func<MDFDoorBuilder> _doorBuilderFactory;
    private readonly CabinetDoorGaps _doorGaps;
    private readonly MDFDoorOptions _mdfOptions;

    public BlindWallCabinetDoorTests() {

        var doorConfiguration = new MDFDoorConfiguration() {
            TopRail = Dimension.Zero,
            BottomRail = Dimension.Zero,
            LeftStile = Dimension.Zero,
            RightStile = Dimension.Zero,
            EdgeDetail = "",
            FramingBead = "",
            Material = ""
        };

        _doorBuilderFactory = () => new(doorConfiguration);

        _doorGaps = new() {
            TopGap = Dimension.FromMillimeters(3),
            BottomGap = Dimension.Zero,
            EdgeReveal = Dimension.FromMillimeters(2),
            HorizontalGap = Dimension.FromMillimeters(3),
            VerticalGap = Dimension.FromMillimeters(3),
        };

        _mdfOptions = new("Style", "Color");

    }

    [Fact]
    public void GetDoors_ShouldReturnCorrectQty_WhenCabinetQtyIsGreatorThan1() {

        int cabinetQty = 2;
        int doorQty = 2;

        // Arrange
        var cabinet = new BlindWallCabinetBuilder()
                            .WithDoors(new() { Quantity = doorQty, MDFOptions = _mdfOptions })
                            .WithBlindWidth(Dimension.FromMillimeters(339))
                            .WithWidth(Dimension.FromMillimeters(685))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .WithQty(cabinetQty)
                            .Build();
        cabinet.DoorGaps = _doorGaps;

        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Sum(d => d.Qty).Should().Be(cabinetQty * doorQty);

    }

    [Fact]
    public void GetDoors_ShouldReturnEmpty_WhenMDFDoorOptionsIsNull() {

        // Arrange
        var cabinet = new BlindWallCabinetBuilder()
                            .WithDoors(new() { Quantity = 1, MDFOptions = null })
                            .WithBlindWidth(Dimension.FromMillimeters(339))
                            .WithWidth(Dimension.FromMillimeters(685))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .Build();
        cabinet.DoorGaps = _doorGaps;

        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().BeEmpty();

    }

    [Theory]
    [InlineData(685, 1, 339, 342.5)]
    [InlineData(993, 2, 339, 323.75)]
    public void DoorWidth_ShouldBeCorrect(double cabWidth, int doorQty, double blindWidth, double expectedDoorWidth) {

        // Arrange
        var cabinet = new BlindWallCabinetBuilder()
                            .WithDoors(new() { Quantity = doorQty, MDFOptions = _mdfOptions })
                            .WithBlindWidth(Dimension.FromMillimeters(blindWidth))
                            .WithWidth(Dimension.FromMillimeters(cabWidth))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .Build();
        cabinet.DoorGaps = _doorGaps;

        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().HaveCount(1);
        doors.First().Width.Should().Be(Dimension.FromMillimeters(expectedDoorWidth));

    }

    [Theory]
    [InlineData(914, 911)]
    [InlineData(876, 873)]
    public void DoorHeight_ShouldBeCorrect(double cabHeight, double expectedDoorHeight) {

        // Arrange
        var cabinet = new BlindWallCabinetBuilder()
                            .WithDoors(new() { Quantity = 1, MDFOptions = _mdfOptions })
                            .WithWidth(Dimension.FromMillimeters(456))
                            .WithHeight(Dimension.FromMillimeters(cabHeight))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .Build();
        cabinet.DoorGaps = _doorGaps;

        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().HaveCount(1);
        doors.First().Height.Should().Be(Dimension.FromMillimeters(expectedDoorHeight));

    }

}