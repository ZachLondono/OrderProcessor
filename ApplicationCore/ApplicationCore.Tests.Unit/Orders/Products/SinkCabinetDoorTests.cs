using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Products;

public class SinkCabinetDoorTests {

    private readonly Func<MDFDoorBuilder> _doorBuilderFactory;
    private readonly MDFDoorOptions _mdfOptions;

    public SinkCabinetDoorTests() {

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

        _mdfOptions = new("Style", "Color");

    }

    [Fact]
    public void GetDoors_ShouldReturnCorrectQty_WhenCabinetQtyIsGreatorThan1() {

        int cabinetQty = 2;
        int doorQty = 1;

        // Arrange
        var cabinet = new SinkCabinetBuilder()
                            .WithMDFOptions(_mdfOptions)
                            .WithDoorQty(doorQty)
                            .WithWidth(Dimension.FromMillimeters(457))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .WithQty(cabinetQty)
                            .Build();
        

        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Sum(d => d.Qty).Should().Be(cabinetQty * doorQty);

    }

    [Fact]
    public void GetDoors_ShouldReturnEmpty_WhenMDFDoorOptionsIsNull() {

        // Arrange
        var cabinet = new SinkCabinetBuilder()
                            .WithMDFOptions(null)
                            .WithDoorQty(1)
                            .WithWidth(Dimension.FromMillimeters(457))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .Build();
        

        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().BeEmpty();

    }

    [Theory]
    [InlineData(457, 1, 453)]
    [InlineData(762, 2, 377.5)]
    public void DoorWidth_ShouldBeCorrect(double cabWidth, int doorQty, double expectedDoorWidth) {

        // Arrange
        var cabinet = new SinkCabinetBuilder()
                            .WithMDFOptions(_mdfOptions)
                            .WithDoorQty(doorQty)
                            .WithWidth(Dimension.FromMillimeters(cabWidth))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .Build();
        

        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().HaveCount(1);
        doors.First().Width.Should().Be(Dimension.FromMillimeters(expectedDoorWidth));

    }

    [Theory]
    [InlineData(457, 1, 453)]
    [InlineData(762, 2, 377.5)]
    public void DrawerWidth_ShouldBeCorrect_WithMultipleDrawers(double cabWidth, int drawerQty, double expectedDrwWidth) {

        // Arrange
        var cabinet = new SinkCabinetBuilder()
                            .WithMDFOptions(_mdfOptions)
                            .WithDoorQty(1)
                            .WithFalseDrawerQty(drawerQty)
                            .WithWidth(Dimension.FromMillimeters(cabWidth))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .Build();
        

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
        var cabinet = new SinkCabinetBuilder()
                            .WithToeType(new TestToeType(Dimension.FromMillimeters(toeHeight)))
                            .WithMDFOptions(_mdfOptions)
                            .WithDoorQty(1)
                            .WithWidth(Dimension.FromMillimeters(456))
                            .WithHeight(Dimension.FromMillimeters(cabHeight))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .Build();
        


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
        var cabinet = new SinkCabinetBuilder()
                            .WithToeType(new TestToeType(Dimension.FromMillimeters(toeHeight)))
                            .WithMDFOptions(_mdfOptions)
                            .WithDoorQty(1)
                            .WithFalseDrawerQty(1)
                            .WithDrawerFaceHeight(Dimension.FromMillimeters(drawerFaceHeight))
                            .WithWidth(Dimension.FromMillimeters(456))
                            .WithHeight(Dimension.FromMillimeters(cabHeight))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .Build();
        


        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().HaveCount(2);
        doors.First().Height.Should().Be(Dimension.FromMillimeters(expectedDoorHeight));
        doors.Skip(1).First().Height.Should().Be(Dimension.FromMillimeters(drawerFaceHeight));

    }

}