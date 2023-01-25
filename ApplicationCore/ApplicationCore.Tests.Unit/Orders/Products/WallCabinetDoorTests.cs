using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Products;

public class WallCabinetDoorTests {

    private readonly Func<MDFDoorBuilder> _doorBuilderFactory;
    private readonly CabinetDoorGaps _doorGaps;
    private readonly MDFDoorOptions _mdfOptions;

    public WallCabinetDoorTests() {

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

    [Theory]
    [InlineData(914, 911)]
    [InlineData(756, 753)]
    public void DoorHeightTests(double cabHeight, double expectedDoorHeight) {

        // Arrange
        var cabinet = new WallCabinetBuilder()
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



    [Theory]
    [InlineData(406, 1, 402)]
    [InlineData(610, 2, 301.5)]
    public void DoorWidthTests(double cabWidth, int doorQty, double expectedDoorWidth) {

        // Arrange
        var cabinet = new WallCabinetBuilder()
                            .WithDoors(new() { Quantity = doorQty, MDFOptions = _mdfOptions })
                            .WithWidth(Dimension.FromMillimeters(cabWidth))
                            .WithHeight(Dimension.FromMillimeters(914))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .Build();
        cabinet.DoorGaps = _doorGaps;


        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().HaveCount(1);
        doors.First().Width.Should().Be(Dimension.FromMillimeters(expectedDoorWidth));

    }

}
