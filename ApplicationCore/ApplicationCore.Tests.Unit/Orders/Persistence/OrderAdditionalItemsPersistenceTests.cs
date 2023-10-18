using ApplicationCore.Features.Orders.Delete;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.State;
using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Data;
using ApplicationCore.Shared.Data.Ordering;
using Dapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ApplicationCore.Tests.Unit.Orders.Persistence;

public class OrderAdditionalItemsPersistenceTests {

    protected readonly IOrderingDbConnectionFactory Factory = new TestOrderingConnectionFactory("./Application/Schemas/ordering_schema.sql");
    private readonly ILogger<InsertOrder.Handler> _logger = Substitute.For<ILogger<InsertOrder.Handler>>();
    private readonly IBus _bus = Substitute.For<IBus>();

    private readonly InsertOrder.Handler _insertOrder;
    private readonly DeleteOrder.Handler _deleteOrder;
    private readonly RemoveAdditionalItem.Handler _removeAdditionalItem;

    public OrderAdditionalItemsPersistenceTests() {
        _insertOrder = new(_logger, Factory, _bus);
        _deleteOrder = new(Factory);
        _removeAdditionalItem = new(Factory);
        SqlMapping.AddSqlMaps();
    }

    [Fact]
    public void InsertOrder_ShouldInsertRowsIntoAdditionalItemsTable() {

        // Arrange
        var item = new AdditionalItem(Guid.NewGuid(), "Test Item", 123.45M);
        var order = new OrderBuilder() {
            Items = new() {
                item
            }
        }.Build();

        // Act
        var result = _insertOrder.Handle(new(order)).Result;
        result.OnError(error => Assert.Fail("Handler returned error"));

        // Assert
        var connection = Factory.CreateConnection().Result;
        var items = connection.QuerySingle<int>("SELECT COUNT(*) FROM additional_items");

        items.Should().Be(1);

    }

    [Fact]
    public void DeleteOrder_ShouldRemoveAdditionalItemsFromTable() {

        // Arrange
        var item = new AdditionalItem(Guid.NewGuid(), "Test Item", 123.45M);
        var order = new OrderBuilder() {
            Items = new() {
                item
            }
        }.Build();

        // Act
        var result = _insertOrder.Handle(new(order)).Result;
        result.OnError(error => Assert.Fail("Handler returned error"));

        var deleteResult = _deleteOrder.Handle(new(order.Id)).Result;
        deleteResult.OnError(error => Assert.Fail("Handler returned error"));

        // Assert
        var connection = Factory.CreateConnection().Result;
        var items = connection.QuerySingle<int>("SELECT COUNT(*) FROM additional_items");

        items.Should().Be(0);

    }

    [Fact]
    public void RemoveAdditionalItem_ShouldRemoveAdditionalItemsFromTable() {

        // Arrange
        var item = new AdditionalItem(Guid.NewGuid(), "Test Item", 123.45M);
        var order = new OrderBuilder() {
            Items = new() {
                item
            }
        }.Build();

        // Act
        var result = _insertOrder.Handle(new(order)).Result;
        result.OnError(error => Assert.Fail("Handler returned error"));

        var deleteResult = _removeAdditionalItem.Handle(new(item.Id)).Result;
        deleteResult.OnError(error => Assert.Fail("Handler returned error"));

        // Assert
        var connection = Factory.CreateConnection().Result;
        var items = connection.QuerySingle<int>("SELECT COUNT(*) FROM additional_items");

        items.Should().Be(0);

    }

}