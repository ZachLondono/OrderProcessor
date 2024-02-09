using Domain.ValueObjects;
using FluentAssertions;
using OrderLoading.ClosetProCSVCutList;
using OrderLoading.ClosetProCSVCutList.CSVModels;

namespace OrderLoading.Tests.Unit.ClosetProOrderLoading;

public class CubbyAccumulatorTests {

    [Fact]
    public void TwoXTwoCubby() {

        // Arrange
        Dimension cubbyDepth = Dimension.FromInches(10);
        Dimension cubbyHeight = Dimension.FromInches(10);
        Dimension cubbyWidth = Dimension.FromInches(10);

        var topShelf = new Part() {
            Depth = cubbyDepth.AsInches(),
            Width = cubbyWidth.AsInches(),
            Color = "White",
            PartCost = "123.45",
            Quantity = 1
        };

        var bottomShelf = new Part() {
            Depth = cubbyDepth.AsInches(),
            Width = cubbyWidth.AsInches(),
            Color = "White",
            PartCost = "123.45",
            Quantity = 1
        };

        var verticalDivider = new Part() {
            Depth = cubbyDepth.AsInches(),
            Height = cubbyHeight.AsInches(),
            Color = "White",
            PartCost = "123.45",
            Quantity = 1
        };

        decimal shelfPrice = 50;
        var horizontalDivider = new Part() {
            Depth = cubbyDepth.AsInches(),
            Width = cubbyWidth.AsInches(),
            Color = "White",
            PartCost = shelfPrice.ToString(),
            Quantity = 1
        };

        var accum = new CubbyAccumulator();
        accum.AddTopShelf(topShelf);
        accum.AddBottomShelf(bottomShelf);
        accum.AddHorizontalPanel(horizontalDivider);
        accum.AddVerticalPanel(verticalDivider);

        // Act
        var cubby = accum.CreateCubby(RoomNamingStrategy.ByWallAndSection);

        // Assert
        cubby.FixedShelves.Sum(s => s.Qty).Should().Be(2);
        var fixedShelf = cubby.FixedShelves.First();
        fixedShelf.UnitPrice.Should().Be(shelfPrice / 2);
        fixedShelf.Width.Should().Be((cubbyWidth - Dimension.FromInches(0.75)) / 2);
        fixedShelf.Depth.Should().Be(cubbyDepth);

        cubby.DividerPanels.Should().HaveCount(1);
        var vertDivider = cubby.DividerPanels.First();
        vertDivider.Depth.Should().Be(cubbyDepth);
        vertDivider.Height.Should().Be(cubbyHeight);

        cubby.TopDividerShelf.Width.Should().Be(cubbyWidth);
        cubby.TopDividerShelf.Depth.Should().Be(cubbyDepth);

        cubby.BottomDividerShelf.Width.Should().Be(cubbyWidth);
        cubby.BottomDividerShelf.Depth.Should().Be(cubbyDepth);

    }

}
