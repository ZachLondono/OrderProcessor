using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Products;

public class BlindBaseCabinetDoorTests {

    private readonly Func<MDFDoorBuilder> _doorBuilderFactory;
    private readonly CabinetDoorGaps _doorGaps;
    private readonly MDFDoorOptions _mdfOptions;

    public BlindBaseCabinetDoorTests() {

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

        _mdfOptions = new("Style", "Color");

    }

    [Fact]
    public void GetDoors_ShouldReturnCorrectQty_WhenCabinetQtyIsGreatorThan1() {

        int cabinetQty = 2;
        int doorQty = 2;
        int drawerQty = 2;

        // Arrange
        var cabinet = new BlindBaseCabinetBuilder()
                            .WithDoors(new() { Quantity = doorQty, MDFOptions = _mdfOptions })
                            .WithDrawers(new() {
                                BoxMaterial = Features.Orders.Shared.Domain.Enums.CabinetDrawerBoxMaterial.FingerJointBirch,
                                Quantity = drawerQty,
                                SlideType = Features.Orders.Shared.Domain.Enums.DrawerSlideType.UnderMount,
                                FaceHeight = Dimension.FromMillimeters(157)
                            })
                            .WithBlindWidth(Dimension.FromMillimeters(635))
                            .WithWidth(Dimension.FromMillimeters(1067))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .WithQty(cabinetQty)
                            .Build();
        cabinet.DoorGaps = _doorGaps;

        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Sum(d => d.Qty).Should().Be(cabinetQty * (doorQty + drawerQty));

    }

    [Fact]
    public void GetDoors_ShouldReturnEmpty_WhenMDFDoorOptionsIsNull() {

        // Arrange
        var cabinet = new BlindBaseCabinetBuilder()
                            .WithDoors(new() { Quantity = 1, MDFOptions = null })
                            .WithDrawers(new() {
                                BoxMaterial = Features.Orders.Shared.Domain.Enums.CabinetDrawerBoxMaterial.FingerJointBirch,
                                Quantity = 1,
                                SlideType = Features.Orders.Shared.Domain.Enums.DrawerSlideType.UnderMount,
                                FaceHeight = Dimension.FromMillimeters(157)
                            })
                            .WithBlindWidth(Dimension.FromMillimeters(635))
                            .WithWidth(Dimension.FromMillimeters(1067))
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
    [InlineData(1067, 1, 635, 428.5)]
    [InlineData(1369, 2, 635, 363.75)]
    public void DoorWidth_ShouldBeCorrect(double cabWidth, int doorQty, double blindWidth, double expectedDoorWidth) {

        // Arrange
        var cabinet = new BlindBaseCabinetBuilder()
                            .WithDoors(new() { Quantity = doorQty, MDFOptions = _mdfOptions })
                            .WithDrawers(new() {
                                BoxMaterial = Features.Orders.Shared.Domain.Enums.CabinetDrawerBoxMaterial.FingerJointBirch,
                                Quantity = 1,
                                SlideType = Features.Orders.Shared.Domain.Enums.DrawerSlideType.UnderMount,
                                FaceHeight = Dimension.FromMillimeters(157)
                            })
                            .WithBlindWidth(Dimension.FromMillimeters(blindWidth))
                            .WithWidth(Dimension.FromMillimeters(cabWidth))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .Build();
        cabinet.DoorGaps = _doorGaps;

        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().HaveCount(2);
        doors.First().Width.Should().Be(Dimension.FromMillimeters(expectedDoorWidth));

    }

    [Theory]
    [InlineData(1067, 1, 635, 428.5)]
    [InlineData(1369, 2, 635, 363.75)]
    public void DrawerWidth_ShouldBeCorrect_WithMultipleDrawers(double cabWidth, int drawerQty, double blindWidth, double expectedDrwWidth) {

        // Arrange
        var cabinet = new BlindBaseCabinetBuilder()
                            .WithDoors(new() { Quantity = 2, MDFOptions = _mdfOptions })
                            .WithDrawers(new() {
                                BoxMaterial = Features.Orders.Shared.Domain.Enums.CabinetDrawerBoxMaterial.FingerJointBirch,
                                Quantity = drawerQty,
                                SlideType = Features.Orders.Shared.Domain.Enums.DrawerSlideType.UnderMount,
                                FaceHeight = Dimension.FromMillimeters(157)
                            })
                            .WithBlindWidth(Dimension.FromMillimeters(blindWidth))
                            .WithWidth(Dimension.FromMillimeters(cabWidth))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .Build();
        cabinet.DoorGaps = _doorGaps;

        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().HaveCount(2);
        doors.Skip(1).First().Width.Should().Be(Dimension.FromMillimeters(expectedDrwWidth));

    }

    [Theory]
    [InlineData(876, 102, 767)]
    [InlineData(700, 102, 591)]
    public void DoorHeight_ShouldBeCorrect(double cabHeight, double toeHeight, double expectedDoorHeight) {

        // Arrange
        var cabinet = new BlindBaseCabinetBuilder()
                            .WithToeType(new LegLevelers(Dimension.FromMillimeters(toeHeight)))
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
    [InlineData(876, 157, 102, 607)]
    [InlineData(700, 157, 102, 431)]
    public void DoorHeight_ShouldBeCorrect_WhenCabinetHasDrawerFace(double cabHeight, double drawerFaceHeight, double toeHeight, double expectedDoorHeight) {

        // Arrange
        var cabinet = new BlindBaseCabinetBuilder()
                            .WithToeType(new LegLevelers(Dimension.FromMillimeters(toeHeight)))
                            .WithDoors(new() { Quantity = 1, MDFOptions = _mdfOptions })
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
