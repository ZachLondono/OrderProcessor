using ApplicationCore.Features.Orders.Data;
using ApplicationCore.Features.Orders.Loader.Commands;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Infrastructure.Bus;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Tests.Unit.Orders.CreatingNewOrder;

public class OrderTests {

    private readonly CreateNewOrder.Handler _sut;
    private readonly ILogger<CreateNewOrder.Handler> _logger = Substitute.For<ILogger<CreateNewOrder.Handler>>();
    private readonly IOrderingDbConnectionFactory _factory = new TestOrderingConnectionFactory("./ordering_schema.sql");
    private readonly IBus _bus = Substitute.For<IBus>();

    public OrderTests() {

        _sut = new(_logger, _factory, _bus);

    }

    /*[Fact]
    public void Should_Insert() {

        // Arrange
        var order = new OrderBuilder().Buid();

        // Act
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
        var task = _sut.Handle(new("", "", "", new() { Name = "" }, Guid.NewGuid(), "", DateTime.Now, shippingInfo, billingInfo, 0, 0, false, new Dictionary<string, string>(), Enumerable.Empty<IProduct>(), Enumerable.Empty<AdditionalItem>()));

        // Assert
        

    }*/

}
