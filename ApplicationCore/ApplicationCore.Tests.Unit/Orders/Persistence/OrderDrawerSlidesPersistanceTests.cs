using ApplicationCore.Features.DeleteOrder;
using Domain.Orders.Persistance;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Domain.Infrastructure.Data;
using Domain.Orders.Entities.Hardware;
using Domain.Orders.Persistance.Repositories;
using Domain.ValueObjects;

namespace ApplicationCore.Tests.Unit.Orders.Persistence;

public class OrderDrawerSlidesPersistanceTests {

    protected readonly IOrderingDbConnectionFactory Factory = new TestOrderingConnectionFactory("./Application/Schemas/ordering_schema.sql");
    private readonly ILogger<InsertOrder.Handler> _logger = Substitute.For<ILogger<InsertOrder.Handler>>();

    private readonly InsertOrder.Handler _insertOrder;
    private readonly DeleteOrder.Handler _deleteOrder;

    public OrderDrawerSlidesPersistanceTests() {
        _insertOrder = new(_logger, Factory);
        _deleteOrder = new(Factory);
        SqlMapping.AddSqlMaps();
    }

    [Fact]
    public async Task InsertOrder_WithDrawerSlides_ShouldInsertRowsIntoSuppliesTable() {

        // Arrange
        var slides = new DrawerSlide(Guid.NewGuid(), 1, Dimension.FromInches(21), "Hettich");
        var order = new OrderBuilder() {
            Hardware = new([], [slides], [])
        }.Build();

        // Act
        var result = await _insertOrder.Handle(new(order));
        result.OnError(error => Assert.Fail("Handler returned error"));

        // Assert
        var connection = Factory.CreateConnection().Result;
        var repo = new OrderDrawerSlidesRepository(connection);
        var orderSlides = repo.GetOrderDrawerSlides(order.Id);
        orderSlides.Should().ContainEquivalentOf(slides);

    }

    [Fact]
    public async Task DeleteOrder_WithSupplies_ShouldDeleteRowsIntoSuppliesTable() {

        // Arrange
        var slides = new DrawerSlide(Guid.NewGuid(), 1, Dimension.FromInches(21), "Hettich");
        var order = new OrderBuilder() {
            Hardware = new([], [slides], [])
        }.Build();

        var result = await _insertOrder.Handle(new(order));
        result.OnError(error => Assert.Fail("Handler returned error"));

        // Act
        var result2 = await _deleteOrder.Handle(new(order.Id));
        result2.OnError(error => Assert.Fail("Handler returned error"));

        // Assert
        var connection = Factory.CreateConnection().Result;
        var repo = new OrderDrawerSlidesRepository(connection);
        var orderSlides = repo.GetOrderDrawerSlides(order.Id);
        orderSlides.Should().BeEmpty();

    }

}
