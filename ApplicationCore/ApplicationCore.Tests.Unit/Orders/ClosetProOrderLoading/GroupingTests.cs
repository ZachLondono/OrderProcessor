using ApplicationCore.Features.ClosetProCSVCutList;
using ApplicationCore.Features.ClosetProCSVCutList.Products;
using ApplicationCore.Shared.Domain;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders.ClosetProOrderLoading;

public class GroupingTests {

    [Fact]
    public void GroupDrawerBoxes() {

        // Arrange
        int qty1 = 3;
        int qty2 = 2;
        var box1 = new DrawerBoxBuilder() {
            Qty = qty1,
            PartNumber = 1,
        }.ToDrawerBox();

        var box2 = new DrawerBoxBuilder() {
            Qty = qty2,
            PartNumber = 2,
        }.ToDrawerBox();

        var sut = new ClosetProGroupAccumulator();
        sut.AddProduct(box1);
        sut.AddProduct(box2);

        // Act
        var products = sut.GetGroupedProducts();

        // Assert
        products.Should().HaveCount(1);
        var groupedBoxes = products.First() as DrawerBox;

        groupedBoxes!.Qty.Should().Be(qty1 + qty2);
        groupedBoxes.Room.Should().Be(box1.Room);
        groupedBoxes.UnitPrice.Should().Be(box1.UnitPrice);
        groupedBoxes.PartNumber.Should().Be(box1.PartNumber);
        groupedBoxes.Height.Should().Be(box1.Height);
        groupedBoxes.Width.Should().Be(box1.Width);
        groupedBoxes.Depth.Should().Be(box1.Depth);
        groupedBoxes.ScoopFront.Should().Be(box1.ScoopFront);
        groupedBoxes.UnderMountNotches.Should().Be(box1.UnderMountNotches);
        groupedBoxes.Type.Should().Be(box1.Type);

    }

    public class DrawerBoxBuilder() {
        public int Qty { get; init; } = 1;
        public string Room { get; init; } = "room";
        public decimal UnitPrice { get; init; } = 123.45M;
        public int PartNumber { get; init; } = 1;
        public Dimension Height { get; init; } = Dimension.Zero;
        public Dimension Width { get; init; } = Dimension.Zero;
        public Dimension Depth { get; init; } = Dimension.Zero;
        public bool ScoopFront { get; init; } = false;
        public bool UnderMountNotches { get; init; } = false;
        public DrawerBoxType Type { get; init; } = DrawerBoxType.Dovetail;
        public DrawerBox ToDrawerBox() {
            return new() {
                Qty = Qty,
                Room = Room,
                UnitPrice = UnitPrice,
                PartNumber = PartNumber,
                Height = Height,
                Width = Width,
                Depth = Depth,
                ScoopFront = ScoopFront,
                UnderMountNotches = UnderMountNotches,
                Type = Type,
            };
        }
    }

}
