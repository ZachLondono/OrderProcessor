using ApplicationCore.Features.Orders.Domain;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders;

public class OrderTests {

    [Fact]
    public void Total_Should_EqualAllItems() {

        // Arrange
        var option = new DrawerBoxOption(Guid.NewGuid(), "");
        var boxes = new List<DrawerBox>() {
            new(Guid.NewGuid(), 1, 12.3M, 5, Dimension.FromMillimeters(0), Dimension.FromMillimeters(0), Dimension.FromMillimeters(0), new(option, option, option, option, option)),
            new(Guid.NewGuid(), 1, 32.1M, 3, Dimension.FromMillimeters(0), Dimension.FromMillimeters(0), Dimension.FromMillimeters(0), new(option, option, option, option, option))
        };
        var items = new List<AdditionalItem>() {
            new(Guid.NewGuid(), "A", 5.43M),
            new(Guid.NewGuid(), "A", 3.45M)
        };
        decimal tax = 54.321M;
        decimal shipping = 123.45M;
        decimal priceAdjustment = 29.29M;
        var order = new Order(Guid.NewGuid(), "", Status.Pending, "", "", Guid.NewGuid(), Guid.NewGuid(), "", "", DateTime.Now, null, null, null, tax, shipping, priceAdjustment, new Dictionary<string, string>(), boxes, items);

        // Act
        var subtotal = order.SubTotal;
        var total = order.Total;
        var adjSubTotal = order.AdjustedSubTotal;

        // Assert
        subtotal.Should().Be(boxes.Sum(b => b.UnitPrice * b.Qty) + items.Sum(i => i.Price));
        total.Should().Be(boxes.Sum(b => b.UnitPrice * b.Qty) + items.Sum(i => i.Price) + tax + shipping);
        adjSubTotal.Should().Be(subtotal + priceAdjustment);

    }

}
