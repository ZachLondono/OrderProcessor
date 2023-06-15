using ApplicationCore.Features.Orders.Delete;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;
using ApplicationCore.Infrastructure.Bus;
using Dapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ApplicationCore.Features.Orders.Shared.State;
using ApplicationCore.Shared.Data.Ordering;
using ApplicationCore.Shared.Data;

namespace ApplicationCore.Tests.Unit.Orders.CreatingNewOrder;

public class OrderTests {

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
        string source = "Order Source";
        string number = "1234";
        string name = "ABC's Kitchen";
        string workingDir = "c:/path/to/directory";
        Guid customerId = Guid.NewGuid();
        Guid vendorId = Guid.NewGuid();
        string comment = "This is a test comment";
        DateTime orderDate = DateTime.Now;
        decimal tax = 123.45M;
        decimal priceAdjustment = 567.123M;
        bool rush = false;
        Dictionary<string, string> info = new();
        List<IProduct> products = new() {
            new DovetailDrawerBoxProduct(Guid.NewGuid(), 0M, 1, "", 1, Dimension.FromInches(4.125), Dimension.FromInches(21), Dimension.FromInches(21), "", new Dictionary<string, string>(), new("MatA", "MatB", "MatC", "MatD", "Clips", "Notches", "Accessory", LogoPosition.None))
        };
        List<AdditionalItem> items = new() {
            new(Guid.NewGuid(), "ABC", 123M),
            new(Guid.NewGuid(), "ABC", 123M),
            new(Guid.NewGuid(), "ABC", 123M)
        };
        var shippingInfo = new ShippingInfo() {
            Address = new(),
            Contact = "",
            Method = "",
            PhoneNumber = "",
            Price = 0
        };
        var billingInfo = new BillingInfo() {
            Address = new(),
            InvoiceEmail = "",
            PhoneNumber = ""
        };

        // Act
        var insertResult = _sut.Handle(new(source, number, name, workingDir, customerId, vendorId, comment, orderDate, shippingInfo, billingInfo, tax, priceAdjustment, rush, info, products, items)).Result;
        Order newOrder;
        insertResult.Match(
            order => newOrder = order,
            error => Assert.Fail("Handler returned error"));

        // Assert
        var connection = _factory.CreateConnection().Result;
        var result = connection.Query("SELECT * FROM orders;");

        result.Should().HaveCount(1);
        var data = result.First();
        ((string)data.source).Should().Be(source);
        ((string)data.number).Should().Be(number);

        var addressResult = connection.Query("SELECT * FROM addresses;");
        addressResult.Should().HaveCount(2);

        var itemsResult = connection.Query("SELECT * FROM additional_items;");
        itemsResult.Should().HaveCount(items.Count);

        var productsResult = connection.Query("SELECT * FROM products;");
        productsResult.Should().HaveCount(products.Count);

    }

    [Fact]
    public void Should_RemoveAllData_WhenDeletingOrder() {

        // Arrange
        string source = "Order Source";
        string number = "1234";
        string name = "ABC's Kitchen";
        string workingDir = "c:/path/to/directory";
        Guid customerId = Guid.NewGuid();
        Guid vendorId = Guid.NewGuid();
        string comment = "This is a test comment";
        DateTime orderDate = DateTime.Now;
        decimal tax = 123.45M;
        decimal priceAdjustment = 567.123M;
        bool rush = false;
        Dictionary<string, string> info = new();

        List<IProduct> products = new() {
            new DovetailDrawerBoxProduct(Guid.NewGuid(), 0M, 1, "", 1, Dimension.FromInches(4.125), Dimension.FromInches(21), Dimension.FromInches(21), "", new Dictionary<string, string>(), new("MatA", "MatB", "MatC", "MatD", "Clips", "Notches", "Accessory", LogoPosition.None)),
            new MDFDoorProduct(Guid.NewGuid(), 0M, "", 1, 1, DoorType.Door, Dimension.FromInches(12), Dimension.FromInches(12), "", new(Dimension.FromInches(2)), "MDF", Dimension.FromInches(0.75), "Shaker", "Square", "Flat", Dimension.FromInches(0.5), DoorOrientation.Vertical, new AdditionalOpening[] { new(Dimension.FromInches(1), Dimension.FromInches(1)) }, "Yellow"),
        };

        List<AdditionalItem> items = new() {
            new(Guid.NewGuid(), "ABC", 123M),
            new(Guid.NewGuid(), "ABC", 123M),
            new(Guid.NewGuid(), "ABC", 123M)
        };
        var shippingInfo = new ShippingInfo() {
            Address = new(),
            Contact = "",
            Method = "",
            PhoneNumber = "",
            Price = 0
        };
        var billingInfo = new BillingInfo() {
            Address = new(),
            InvoiceEmail = "",
            PhoneNumber = ""
        };

        var insertResult = _sut.Handle(new(source, number, name, workingDir, customerId, vendorId, comment, orderDate, shippingInfo, billingInfo, tax, priceAdjustment, rush, info, products, items)).Result;
        Order? newOrder = null;
        insertResult.Match(
            order => newOrder = order,
            error => Assert.Fail($"Insert handler returned error {error.Title} - {error.Details}"));
        newOrder.Should().NotBeNull();

        // Act
        var sut = new DeleteOrder.Handler(_factory);
        var deleteResult = sut.Handle(new(newOrder.Id)).Result;
        deleteResult.OnError(e => Assert.Fail("Handler returned error"));

        // Assert
        var connection = _factory.CreateConnection().Result;

        var tableNames = connection.Query<string>("SELECT name FROM sqlite_master WHERE type='table';");
        tableNames.Should().NotBeEmpty("Unexpected result from query");
        foreach (var tableName in tableNames) {
            var itemCount = connection.QuerySingle<int>($"SELECT COUNT(*) FROM {tableName};");
            itemCount.Should().Be(0, $"Found {itemCount} rows in table '{tableName}' when there should be none");
        }


    }


}
