using FluentAssertions;
using NSubstitute;
using ApplicationCore.Features.Orders.Shared.State;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using Microsoft.Extensions.Logging;
using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Tests.Unit.Orders;

public class OrderStateTests {

    private readonly OrderState _sut;
    private readonly IBus _bus = Substitute.For<IBus>();
    private readonly ILogger<OrderState> _logger = Substitute.For<ILogger<OrderState>>();

    public OrderStateTests() {
        _sut = new(_bus, _logger);
    }

    [Fact]
    public void ShouldBeNullByDefault() {

        // Assert
        _sut.Order.Should().BeNull();

    }

    [Fact]
    public void ReplaceOrder_ShouldSetOrder_AndResetIsDirty() {

        // Arrange
        var order = new OrderBuilder().Buid();

        // Act
        _sut.ReplaceOrder(order);

        // Assert
        _sut.Order.Should().Be(order);

    }

    [Fact]
    public void LoadOrder_ShouldCallGetOrder() {

        // Arrange
        var order = new OrderBuilder().Buid();
        var query = new GetOrderById.Query(order.Id);
        var response = new Response<Order>(order);
        _bus.Send(query).Returns(response);

        // Act
        _sut.LoadOrder(order.Id).Wait();

        // Assert
        _sut.Order.Should().NotBeNull();
        _sut.Order.Should().Be(order);
        _bus.Received(1).Send(query);

    }

}
