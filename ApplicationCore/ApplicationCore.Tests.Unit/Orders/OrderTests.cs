using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using FluentAssertions;

namespace ApplicationCore.Tests.Unit.Orders;

public class OrderTests {

    [Fact]
    public void Total_Should_EqualAllItems() {

        // Arrange
        var boxes = new List<DovetailDrawerBoxProduct>() {
            new DrawerBoxBuilder().WithUnitPrice(45.45M).WithQty(2).Build(),
            new DrawerBoxBuilder().WithUnitPrice(16.54M).WithQty(3).Build()
        };

        var items = new List<AdditionalItem>() {
            new AdditionalItemBuilder().WithPrice(12.3M).Build(),
            new AdditionalItemBuilder().WithPrice(32.1M).Build()
        };

        decimal tax = 54.321M;
        decimal shipping = 123.45M;
        decimal priceAdjustment = 29.29M;
        var order = new OrderBuilder()
                            .WithTax(tax)
                            .WithShipping(shipping)
                            .WithPriceAdjustment(priceAdjustment)
                            .WithItems(items)
                            .WithBoxes(boxes)
                            .Buid();

        // Act
        var subtotal = order.SubTotal;
        var total = order.Total;
        var adjSubTotal = order.AdjustedSubTotal;

        // Assert
        subtotal.Should().Be(boxes.Sum(b => b.UnitPrice * b.Qty) + items.Sum(i => i.Price));
        total.Should().Be(boxes.Sum(b => b.UnitPrice * b.Qty) + items.Sum(i => i.Price) + tax + shipping);
        adjSubTotal.Should().Be(subtotal + priceAdjustment);

    }

    [Fact]
    public void Release_ShouldUpdateStatusAndDate() {

        // Arrange
        var order = new OrderBuilder().Buid();

        // Act
        order.Release();

        // Assert
        //order.ReleaseDate.Should().Be(DateTime.Today);
        order.ProductionDate.Should().Be(DateTime.Today.AddDays(7));
        order.Status.Should().Be(Status.Released);

    }

    [Fact]
    public void Release_ShouldUpdateStatusAndDate_WhenProductionDateIsGiven() {

        // Arrange
        var order = new OrderBuilder().Buid();
        DateTime productionDate = DateTime.Today.AddDays(13);

        // Act
        order.Release(productionDate);

        // Assert
        //order.ReleaseDate.Should().Be(DateTime.Today);
        order.ProductionDate.Should().Be(productionDate);
        order.Status.Should().Be(Status.Released);

    }

    [Fact]
    public void Complete_ShouldUpdateStatusAndDate() {

        // Arrange
        var order = new OrderBuilder().Buid();

        // Act
        order.Complete();

        // Assert
        //order.CompleteDate.Should().Be(DateTime.Today);
        order.Status.Should().Be(Status.Completed);

    }

}
