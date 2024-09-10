using ApplicationCore.Features.DeleteOrder;
using Domain.Orders.Persistance;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Domain.Infrastructure.Data;
using Domain.Orders.Entities.Hardware;
using Domain.Orders.Persistance.Repositories;

namespace ApplicationCore.Tests.Unit.Orders.Persistence;

public class OrderSuppliesPersistanceTests {

    /*

    protected readonly IOrderingDbConnectionFactory Factory = new TestOrderingConnectionFactory("./Application/Schemas/ordering_schema.sql");
    private readonly ILogger<InsertOrder.Handler> _logger = Substitute.For<ILogger<InsertOrder.Handler>>();

    private readonly InsertOrder.Handler _insertOrder;
    private readonly DeleteOrder.Handler _deleteOrder;

    public OrderSuppliesPersistanceTests() {
        _insertOrder = new(_logger, Factory);
        _deleteOrder = new(Factory);
        SqlMapping.AddSqlMaps();
    }

    [Fact]
    public async Task InsertOrder_WithSupplies_ShouldInsertRowsIntoSuppliesTable() {

        // Arrange
        var supply = new Supply(Guid.NewGuid(), 1, "Hinge Plates");
        var order = new OrderBuilder() {
            Hardware = new([supply], [], [])
        }.Build();

        // Act
        var result = await _insertOrder.Handle(new(order));
        result.OnError(error => Assert.Fail("Handler returned error"));

        // Assert
        var connection = Factory.CreateConnection().Result;
        var repo = new OrderSuppliesRepository(connection);
        var supplies = repo.GetOrderSupplies(order.Id);
        supplies.Should().ContainEquivalentOf(supply);

    }

    [Fact]
    public async Task DeleteOrder_WithSupplies_ShouldDeleteRowsIntoSuppliesTable() {

        // Arrange
        var supply = new Supply(Guid.NewGuid(), 1, "Hinge Plates");
        var order = new OrderBuilder() {
            Hardware = new([supply], [], [])
        }.Build();

        var result1 = await _insertOrder.Handle(new(order));
        result1.OnError(error => Assert.Fail("Handler returned error"));

        // Act
        var result2 = await _deleteOrder.Handle(new(order.Id));
        result2.OnError(error => Assert.Fail("Handler returned error"));

        // Assert
        var connection = Factory.CreateConnection().Result;
        var repo = new OrderSuppliesRepository(connection);
        var supplies = repo.GetOrderSupplies(order.Id);
        supplies.Should().BeEmpty();

    }

    */

}
