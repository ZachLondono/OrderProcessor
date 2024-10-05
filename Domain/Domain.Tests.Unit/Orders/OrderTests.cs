using Domain.Orders.Entities;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using FluentAssertions;

namespace Domain.Tests.Unit.Orders;

public class OrderTests {

    [Fact]
    public void Total_Should_EqualAllItems() {

        // Arrange
        var boxes = new List<IProduct>() {
            new DrawerBoxBuilder().WithUnitPrice(45.45M).WithQty(2).Build(),
            new DrawerBoxBuilder().WithUnitPrice(16.54M).WithQty(3).Build()
        };

        var items = new List<AdditionalItem>() {
            new AdditionalItemBuilder().WithPrice(12.3M).Build(),
            new AdditionalItemBuilder().WithPrice(32.1M).Build()
        };

        ShippingInfo shipping = new() {
            Price = 123.45M,
            Contact = "",
            Method = "",
            PhoneNumber = "",
            Address = new()
        };

        decimal tax = 54.321M;
        decimal priceAdjustment = 29.29M;
        var order = new OrderBuilder()
                            .WithTax(tax)
                            .WithShipping(shipping)
                            .WithPriceAdjustment(priceAdjustment)
                            .WithItems(items)
                            .WithProducts(boxes)
                            .Build();

        // Act
        var subtotal = order.SubTotal;
        var total = order.Total;
        var adjSubTotal = order.AdjustedSubTotal;

        // Assert
        subtotal.Should().Be(boxes.Sum(b => b.UnitPrice * b.Qty) + items.Sum(i => i.UnitPrice));
        total.Should().Be(boxes.Sum(b => b.UnitPrice * b.Qty) + items.Sum(i => i.UnitPrice) + tax + shipping.Price);
        adjSubTotal.Should().Be(subtotal + priceAdjustment);

    }

}
