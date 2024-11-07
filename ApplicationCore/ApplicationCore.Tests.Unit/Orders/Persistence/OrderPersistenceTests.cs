using ApplicationCore.Features.Orders.Details.Queries;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ApplicationCore.Tests.Unit.Orders.Persistence;

public class OrderPersistenceTests : PersistenceTests {

    [Fact]
    public async Task Should_Insert() {

        // Arrange
        var order = new OrderBuilder() {
            Source = "Test Source",
            Number = "Test Number",
            Name = "TestName",
            Items = new() {
                new(Guid.Empty, 1, "Test Item", 123.45M)
            },
        }.Build();

        // Act
        var insertResult = await Sut.Handle(new(order));
        insertResult.OnError(error => Assert.Fail($"Handler returned error - {error.Title} : {error.Details}"));

        // Assert
        var connection = await Factory.CreateConnection();
        var result = connection.Query("SELECT * FROM orders;");

        result.Should().HaveCount(1);
        var data = result.First();
        ((string)data.source).Should().Be(order.Source);
        ((string)data.number).Should().Be(order.Number);
        ((string)data.name).Should().Be(order.Name);

        var addressResult = connection.Query("SELECT * FROM addresses;");
        addressResult.Should().HaveCount(2);

        var itemsResult = connection.Query("SELECT * FROM additional_items;");
        itemsResult.Should().HaveCount(order.AdditionalItems.Count());

        var productsResult = connection.Query("SELECT * FROM products;");
        productsResult.Should().HaveCount(order.Products.Count());

    }

    [Fact]
    public async Task Should_Insert_And_Select() {

        // Arrange
        var order = new OrderBuilder() {
            Source = "Test Source",
            Number = "Test Number",
            Name = "TestName",
            Items = new() {
                new(Guid.Empty, 1, "Test Item", 123.45M)
            },
        }.Build();

        // Act
        var insertResult = await Sut.Handle(new(order));
        insertResult.OnError(error => Assert.Fail($"Handler returned error - {error.Title} : {error.Details}"));

        // Assert

        var logger = Substitute.For<ILogger<GetOrderById.Handler>>();
        var handler = new GetOrderById.Handler(logger, Factory);
        var getResult = await handler.Handle(new(order.Id));

        order.Should().BeEquivalentTo(getResult.Value!);

    }

}
