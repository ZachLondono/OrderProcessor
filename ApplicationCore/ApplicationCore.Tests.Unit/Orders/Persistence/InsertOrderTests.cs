using ApplicationCore.Features.Orders.Delete;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Infrastructure.Bus;
using Dapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ApplicationCore.Features.Orders.Shared.State;
using ApplicationCore.Shared.Data.Ordering;
using ApplicationCore.Shared.Data;

namespace ApplicationCore.Tests.Unit.Orders.Persistence;

public partial class OrderTests {

    private readonly InsertOrder.Handler _sut;
    private readonly ILogger<InsertOrder.Handler> _logger = Substitute.For<ILogger<InsertOrder.Handler>>();
    private readonly IOrderingDbConnectionFactory _factory = new TestOrderingConnectionFactory("./Application/Schemas/ordering_schema.sql");
    private readonly IBus _bus = Substitute.For<IBus>();

    public OrderTests() {

        _sut = new(_logger, _factory, _bus);
        SqlMapping.AddSqlMaps();

    }

    [Fact]
    public void Should_Insert() {

        // Arrange
        var order = new OrderBuilder() {
            Source = "Test Source",
            Number = "Test Number",
            Name = "TestName",
            Items = new() {
                new(Guid.Empty, "Test Item", 123.45M)
            },
        }.Build();

        // Act
        var insertResult = _sut.Handle(new(order)).Result;
        insertResult.OnError(error => Assert.Fail("Handler returned error"));

        // Assert
        var connection = _factory.CreateConnection().Result;
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

    protected void InsertAndQueryOrderWithProduct<T>(T product) where T : IProduct {

        Order order = new OrderBuilder() {
            Products = new() {
                product 
            }
        }.Build();

        var insertResult = _sut.Handle(new(order)).Result;
        insertResult.OnError(error => Assert.Fail($"Insert handler returned error {error.Title} - {error.Details}"));
        order.Should().NotBeNull();

        var logger = Substitute.For<ILogger<GetOrderById.Handler>>();
        var sut = new GetOrderById.Handler(logger, _factory);

        // Act
        var result = sut.Handle(new(order.Id)).Result;
        Order? foundOrder = null;
        result.OnSuccess(o => {
            foundOrder = o;
        });

        // Assert
        result.OnError(e => Assert.Fail($"Handler returned error {e}"));
        foundOrder.Should().NotBeNull();

        var foundProduct = (T) foundOrder.Products.First();
        foundProduct.Should().NotBeNull();
        foundProduct.Should().BeEquivalentTo(product);

    }

    protected void InsertAndDeleteOrderWithProduct<T>(T product) where T : IProduct {

        Order order = new OrderBuilder() {
            Products = new() {
                product
            }
        }.Build();

        var insertResult = _sut.Handle(new(order)).Result;
        insertResult.OnError(error => Assert.Fail($"Insert handler returned error {error.Title} - {error.Details}"));
        order.Should().NotBeNull();

        var sut = new DeleteOrder.Handler(_factory);

        // Act
        var deleteResult = sut.Handle(new(order.Id)).Result;

        // Assert
        deleteResult.OnError(e => Assert.Fail("Handler returned error"));
        EnsureAllTablesAreEmpty();

    }

    protected void EnsureAllTablesAreEmpty() { 
        var connection = _factory.CreateConnection().Result;

        var tableNames = connection.Query<string>("SELECT name FROM sqlite_master WHERE type='table';");
        tableNames.Should().NotBeEmpty("Unexpected result from query");
        foreach (var tableName in tableNames) {
            var itemCount = connection.QuerySingle<int>($"SELECT COUNT(*) FROM {tableName};");
            itemCount.Should().Be(0, $"Found {itemCount} rows in table '{tableName}' when there should be none");
        }


    }

}