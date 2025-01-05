using Domain.Companies.ValueObjects;
using Domain.Orders.Entities.Products.Closets;
using Domain.ValueObjects;
using FluentAssertions;
using OrderLoading.ClosetProCSVCutList.Products.Shelves;

namespace OrderLoading.Tests.Unit.ClosetProOrderLoading;

public class ShelfToProductTests {

    private ClosetProSettings _settings = new() {
        AdjustableShelfSKU = "SA",
        FixedShelfSKU = "SF",
    };

    [Fact]
    public void FixedShelfNoLED() {

        var shelf = CreateShelf(Dimension.FromInches(12), ShelfType.Fixed, false);

        var product = shelf.ToProduct(_settings) as ClosetPart;

        product!.SKU.Should().Be("SF");

    }

    [Fact]
    public void FixedShelfWithLED() {

        var shelf = CreateShelf(Dimension.FromInches(12), ShelfType.Fixed, true);

        var product = shelf.ToProduct(_settings) as ClosetPart;

        product!.SKU.Should().Be("SF-LED");

    }

    [Fact]
    public void AdjustableShelfNoLED() {

        var shelf = CreateShelf(Dimension.FromInches(12), ShelfType.Adjustable, false);

        var product = shelf.ToProduct(_settings) as ClosetPart;

        product!.SKU.Should().Be("SA");

    }

    [Fact]
    public void AdjustableShelfWithLED() {

        var shelf = CreateShelf(Dimension.FromInches(12), ShelfType.Adjustable, true);

        var product = shelf.ToProduct(_settings) as ClosetPart;

        product!.SKU.Should().Be("SA-LED");

    }

    [Fact]
    public void ShoeShelfWithLED() {

        var shelf = CreateShelf(Dimension.FromInches(12), ShelfType.Shoe, true);

        var action = () => shelf.ToProduct(_settings);

        action.Should().Throw<NotSupportedException>();

    }

    [Theory]
    [InlineData(12)]
    [InlineData(14)]
    [InlineData(16)]
    public void ShoeShelfNoLED(double depthIn) {

        var shelf = CreateShelf(Dimension.FromInches(depthIn), ShelfType.Shoe, false);

        var product = shelf.ToProduct(_settings) as ClosetPart;

        product!.SKU.Should().Be($"SS{depthIn:0}-TAG");

    }

    public static Shelf CreateShelf(Dimension depth, ShelfType type, bool ledChannel) {
        return new Shelf() {
            Qty = 1,
            Color = "White",
            EdgeBandingColor = "White",
            Room = "Room",
            UnitPrice = 0,
            PartNumber = 1,
            Width = Dimension.FromInches(12),
            Depth = depth,
            ExtendBack = false,
            Type = type,
            LEDChannel = ledChannel 
        };
    }


}
