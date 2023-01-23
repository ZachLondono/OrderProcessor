using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Products;

public class BaseCabinetTests {

    private readonly Func<MDFDoorBuilder> _doorBuilderFactory;
    private readonly CabinetDoorGaps _doorGaps;

    public BaseCabinetTests() {

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
            TopGap = Dimension.FromMillimeters(7),
            BottomGap = Dimension.Zero,
            EdgeReveal = Dimension.FromMillimeters(2),
            HorizontalGap = Dimension.FromMillimeters(3),
            VerticalGap = Dimension.FromMillimeters(3),
        };

    }

    [Theory]
    [InlineData(457, 1, 453)]
    [InlineData(762, 2, 377.5)]
    public void DoorWidth_ShouldBeCorrect(double cabWidth, int doorQty, double expectedDoorWidth) {

        // Arrange
        var cabinet = new BaseCabinetBuilder()
                            .WithDoors(new() { Quantity = doorQty })
                            .WithDrawers(new() {
                                BoxMaterial = Features.Orders.Shared.Domain.Enums.CabinetDrawerBoxMaterial.FingerJointBirch,
                                Quantity = 1,
                                SlideType = Features.Orders.Shared.Domain.Enums.DrawerSlideType.UnderMount,
                                FaceHeight = Dimension.FromMillimeters(157)
                            })
                            .WithWidth(Dimension.FromMillimeters(cabWidth))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .Build();
        cabinet.DoorGaps = _doorGaps;

        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().HaveCount(2);
        doors.First().Qty.Should().Be(doorQty);
        doors.First().Width.Should().Be(Dimension.FromMillimeters(expectedDoorWidth));

    }

    [Theory]
    [InlineData(457, 1, 453)]
    [InlineData(762, 2, 377.5)]
    public void DrawerWidth_ShouldBeCorrect_WithMultipleDrawers(double cabWidth, int drawerQty, double expectedDrwWidth) {

        // Arrange
        var cabinet = new BaseCabinetBuilder()
                            .WithDoors(new() { Quantity = 2 })
                            .WithDrawers(new() {
                                BoxMaterial = Features.Orders.Shared.Domain.Enums.CabinetDrawerBoxMaterial.FingerJointBirch,
                                Quantity = drawerQty,
                                SlideType = Features.Orders.Shared.Domain.Enums.DrawerSlideType.UnderMount,
                                FaceHeight = Dimension.FromMillimeters(157)
                            })
                            .WithWidth(Dimension.FromMillimeters(cabWidth))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .Build();
        cabinet.DoorGaps = _doorGaps;

        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().HaveCount(2);
        doors.Skip(1).First().Qty.Should().Be(drawerQty);
        doors.Skip(1).First().Width.Should().Be(Dimension.FromMillimeters(expectedDrwWidth));

    }

    [Theory]
    [InlineData(876, 102, 767)]
    [InlineData(700, 102, 591)]
    public void DoorHeight_ShouldBeCorrect(double cabHeight, double toeHeight, double expectedDoorHeight) {

        // Arrange
        var cabinet = new BaseCabinetBuilder()
                            .WithToeType(new LegLevelers(Dimension.FromMillimeters(toeHeight)))
                            .WithDoors(new() { Quantity = 1 })
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
    [InlineData(876, 157, 102, 607)]
    [InlineData(700, 157, 102, 431)]
    public void DoorHeight_ShouldBeCorrect_WhenCabinetHasDrawerFace(double cabHeight, double drawerFaceHeight, double toeHeight, double expectedDoorHeight) {

        // Arrange
        var cabinet = new BaseCabinetBuilder()
                            .WithToeType(new LegLevelers(Dimension.FromMillimeters(toeHeight)))
                            .WithDoors(new() { Quantity = 1 })
                            .WithDrawers(new() {
                                BoxMaterial = Features.Orders.Shared.Domain.Enums.CabinetDrawerBoxMaterial.FingerJointBirch,
                                Quantity = 1,
                                SlideType = Features.Orders.Shared.Domain.Enums.DrawerSlideType.UnderMount,
                                FaceHeight = Dimension.FromMillimeters(drawerFaceHeight)
                            })
                            .WithWidth(Dimension.FromMillimeters(456))
                            .WithHeight(Dimension.FromMillimeters(cabHeight))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .Build();
        cabinet.DoorGaps = _doorGaps;


        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().HaveCount(2);
        doors.First().Height.Should().Be(Dimension.FromMillimeters(expectedDoorHeight));
        doors.Skip(1).First().Height.Should().Be(Dimension.FromMillimeters(drawerFaceHeight));

    }

}
