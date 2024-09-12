using Domain.Orders.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Domain.Orders.Persistance;
using ApplicationCore.Features.Orders.Details.Queries;
using ApplicationCore.Features.DeleteOrder;
using Domain.Orders.Entities.Products;
using Domain.Infrastructure.Data;

namespace ApplicationCore.Tests.Unit.Orders.Persistence;

public abstract class PersistenceTests : IDisposable {

    protected readonly InsertOrder.Handler Sut;
    protected readonly TestOrderingConnectionFactory Factory = new TestOrderingConnectionFactory("./Application/Schemas/ordering_schema.sql");
    private readonly ILogger<InsertOrder.Handler> _logger = Substitute.For<ILogger<InsertOrder.Handler>>();

    public PersistenceTests() {
        Sut = new(_logger, Factory);
        SqlMapping.AddSqlMaps();
    }

    public void Dispose() {
        Factory.Dispose();
    }

    protected void InsertAndQueryOrderWithProduct<T>(T product) where T : IProduct {

        Order order = new OrderBuilder() {
            Products = new() {
                product
            }
        }.Build();

        var insertResult = Sut.Handle(new(order)).Result;
        insertResult.OnError(error => Assert.Fail($"Insert handler returned error {error.Title} - {error.Details}"));
        order.Should().NotBeNull();

        var logger = Substitute.For<ILogger<GetOrderById.Handler>>();
        var sut = new GetOrderById.Handler(logger, Factory);

        // Act
        var result = sut.Handle(new(order.Id)).Result;
        Order? foundOrder = null;
        result.OnSuccess(o => {
            foundOrder = o;
        });

        // Assert
        result.OnError(e => Assert.Fail($"Handler returned error {e}"));
        foundOrder.Should().NotBeNull();

        var foundProduct = (T)foundOrder.Products.First();
        foundProduct.Should().NotBeNull();
        foundProduct.Should().BeEquivalentTo(product);

    }

    protected void InsertAndDeleteOrderWithProduct<T>(T product) where T : IProduct {

        Order order = new OrderBuilder() {
            Products = new() {
                product
            }
        }.Build();

        var insertResult = Sut.Handle(new(order)).Result;
        insertResult.OnError(error => Assert.Fail($"Insert handler returned error {error.Title} - {error.Details}"));
        order.Should().NotBeNull();

        var sut = new DeleteOrder.Handler(Factory);

        // Act
        var deleteResult = sut.Handle(new(order.Id)).Result;

        // Assert
        deleteResult.OnError(e => Assert.Fail("Handler returned error"));
        EnsureAllTablesAreEmpty();

    }

    protected Guid InsertOrderWithProduct<T>(T product) where T : IProduct {

        Order order = new OrderBuilder() {
            Products = new() {
                product
            }
        }.Build();

        var insertResult = Sut.Handle(new(order)).Result;
        insertResult.OnError(error => Assert.Fail($"Insert handler returned error {error.Title} - {error.Details}"));
        order.Should().NotBeNull();

        return order.Id;

    }

    protected void VerifyProductExistsInOrder<T>(Guid orderId, T product) where T : IProduct {

        var logger = Substitute.For<ILogger<GetOrderById.Handler>>();
        var sut = new GetOrderById.Handler(logger, Factory);

        var result = sut.Handle(new(orderId)).Result;
        Order? foundOrder = null;
        result.OnSuccess(o => {
            foundOrder = o;
        });

        // Assert
        result.OnError(e => Assert.Fail($"Handler returned error {e}"));
        foundOrder.Should().NotBeNull();

        var foundProduct = (T)foundOrder.Products.First();
        foundProduct.Should().NotBeNull();
        foundProduct.Should().BeEquivalentTo(product);

    }

    protected void EnsureAllTablesAreEmpty() {
        var connection = Factory.CreateConnection().Result;

        var tableNames = connection.Query<string>("SELECT name FROM sqlite_master WHERE type='table';");
        tableNames.Should().NotBeEmpty("Unexpected result from query");
        foreach (var tableName in tableNames) {
            var itemCount = connection.QuerySingle<int>($"SELECT COUNT(*) FROM {tableName};");
            itemCount.Should().Be(0, $"Found {itemCount} rows in table '{tableName}' when there should be none");
        }


    }

}
