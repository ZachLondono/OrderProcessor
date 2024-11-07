using ApplicationCore.Features.DeleteOrder;
using Domain.Orders.Entities;
using Domain.Orders.Persistance;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Domain.Infrastructure.Data;

namespace ApplicationCore.Tests.Unit.Orders.Persistence;

public class OrderAdditionalItemsPersistenceTests {

    protected readonly IOrderingDbConnectionFactory Factory = new TestOrderingConnectionFactory("./Application/Schemas/ordering_schema.sql");
    private readonly ILogger<InsertOrder.Handler> _logger = Substitute.For<ILogger<InsertOrder.Handler>>();

    private readonly InsertOrder.Handler _insertOrder;
    private readonly DeleteOrder.Handler _deleteOrder;
    private readonly RemoveAdditionalItem.Handler _removeAdditionalItem;

    public OrderAdditionalItemsPersistenceTests() {
        _insertOrder = new(_logger, Factory);
        _deleteOrder = new(Factory);
        _removeAdditionalItem = new(Factory);
        SqlMapping.AddSqlMaps();
    }

    [Fact]
    public async Task InsertOrder_ShouldInsertRowsIntoAdditionalItemsTable() {

        // Arrange
        var item = new AdditionalItem(Guid.NewGuid(), 1, "Test Item", 123.45M);
        var order = new OrderBuilder() {
            Items = new() {
                item
            }
        }.Build();

        // Act
        var result = await _insertOrder.Handle(new(order));
        result.OnError(error => Assert.Fail("Handler returned error"));

        // Assert
        var connection = await Factory.CreateConnection();
        var items = connection.QuerySingle<int>("SELECT COUNT(*) FROM additional_items");

        items.Should().Be(1);

    }

    [Fact]
    public async Task DeleteOrder_ShouldRemoveAdditionalItemsFromTable() {

        // Arrange
        var item = new AdditionalItem(Guid.NewGuid(), 1, "Test Item", 123.45M);
        var order = new OrderBuilder() {
            Items = new() {
                item
            }
        }.Build();

        // Act
        var result = await _insertOrder.Handle(new(order));
        result.OnError(error => Assert.Fail("Handler returned error"));

        var deleteResult = await _deleteOrder.Handle(new(order.Id));
        deleteResult.OnError(error => Assert.Fail("Handler returned error"));

        // Assert
        var connection = await Factory.CreateConnection();
        var items = connection.QuerySingle<int>("SELECT COUNT(*) FROM additional_items");

        items.Should().Be(0);

    }

    [Fact]
    public async Task RemoveAdditionalItem_ShouldRemoveAdditionalItemsFromTable() {

        // Arrange
        var item = new AdditionalItem(Guid.NewGuid(), 1, "Test Item", 123.45M);
        var order = new OrderBuilder() {
            Items = new() {
                item
            }
        }.Build();

        // Act
        var result = await _insertOrder.Handle(new(order));
        result.OnError(error => Assert.Fail("Handler returned error"));

        var deleteResult = await _removeAdditionalItem.Handle(new(item.Id));
        deleteResult.OnError(error => Assert.Fail("Handler returned error"));

        // Assert
        var connection = await Factory.CreateConnection();
        var items = connection.QuerySingle<int>("SELECT COUNT(*) FROM additional_items");

        items.Should().Be(0);

    }

}