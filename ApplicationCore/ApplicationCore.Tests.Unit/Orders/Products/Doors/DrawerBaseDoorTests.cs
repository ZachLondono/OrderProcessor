using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.Products.Doors;

public class DrawerBaseDoorTests {

    private readonly Func<MDFDoorBuilder> _doorBuilderFactory;
    private readonly MDFDoorOptions _mdfOptions;

    public DrawerBaseDoorTests() {

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
        var cabinet = new DrawerBaseCabinetBuilder()
                            .WithDrawers(new() {
                                FaceHeights = new Dimension[] {
                                    Dimension.FromMillimeters(157),
                                    Dimension.FromMillimeters(157)
                                },
                            })
                            .WithWidth(Dimension.FromMillimeters(453))
                            .WithHeight(Dimension.FromMillimeters(876))
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
        var cabinet = new DrawerBaseCabinetBuilder()
                            .WithDrawers(new() {
                                FaceHeights = new Dimension[] {
                                    Dimension.FromMillimeters(157),
                                    Dimension.FromMillimeters(157)
                                }
                            })
                            .WithWidth(Dimension.FromMillimeters(453))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .WithMDFDoorOptions(null)
                            .Build();


        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.Should().BeEmpty();

    }

    [Theory]
    [InlineData(457, 453)]
    public void DrawerWidthTests(double cabWidth, double expectedDrawerWidth) {

        // Arrange
        var cabinet = new DrawerBaseCabinetBuilder()
                            .WithDrawers(new() {
                                FaceHeights = new Dimension[] {
                                    Dimension.FromMillimeters(157),
                                    Dimension.FromMillimeters(157)
                                }
                            })
                            .WithWidth(Dimension.FromMillimeters(cabWidth))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .WithMDFDoorOptions(_mdfOptions)
                            .Build();


        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.First().Width.Should().Be(Dimension.FromMillimeters(expectedDrawerWidth));

    }

    [Theory]
    [InlineData(157, 158, 159, 160, 161)]
    public void DrawerHeightTests(double face1, double face2, double face3, double face4, double face5) {

        // Arrange
        var cabinet = new DrawerBaseCabinetBuilder()
                            .WithDrawers(new() {
                                FaceHeights = new Dimension[] {
                                    Dimension.FromMillimeters(face1),
                                    Dimension.FromMillimeters(face2),
                                    Dimension.FromMillimeters(face3),
                                    Dimension.FromMillimeters(face4),
                                    Dimension.FromMillimeters(face5)
                                }
                            })
                            .WithWidth(Dimension.FromMillimeters(457))
                            .WithHeight(Dimension.FromMillimeters(876))
                            .WithDepth(Dimension.FromMillimeters(610))
                            .WithMDFDoorOptions(_mdfOptions)
                            .Build();


        // Act
        var doors = cabinet.GetDoors(_doorBuilderFactory);

        // Assert
        doors.First().Height.Should().Be(Dimension.FromMillimeters(face1));
        doors.Skip(1).First().Height.Should().Be(Dimension.FromMillimeters(face2));
        doors.Skip(2).First().Height.Should().Be(Dimension.FromMillimeters(face3));
        doors.Skip(3).First().Height.Should().Be(Dimension.FromMillimeters(face4));
        doors.Skip(4).First().Height.Should().Be(Dimension.FromMillimeters(face5));

    }

}
