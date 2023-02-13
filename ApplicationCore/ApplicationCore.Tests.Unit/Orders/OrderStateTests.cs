using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.Orders.Release;
using ApplicationCore.Infrastructure;
using FluentAssertions;
using NSubstitute;
using ApplicationCore.Features.Orders.Shared.State;
using ApplicationCore.Features.Companies.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using Microsoft.Extensions.Logging;
using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Tests.Unit.Orders;

public class OrderStateTests {

    private readonly OrderState _sut;
    private readonly IBus _bus = Substitute.For<IBus>();
    private readonly IUIBus _uiBus = Substitute.For<IUIBus>();
    private readonly ILogger<OrderState> _logger = Substitute.For<ILogger<OrderState>>();

    public OrderStateTests() {
        _sut = new(_bus, _uiBus, _logger);
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
    public void Release_ShouldDoNothing_WhenOrderIsNull() {

        // Act
        _sut.Release().Wait();

        // Assert
        _sut.Order.Should().BeNull();
        _bus.DidNotReceiveWithAnyArgs().Send(default(IQuery<ReleaseProfile>)!);
        _bus.DidNotReceiveWithAnyArgs().Send(default(ICommand<ReleaseProfile>)!);
        _bus.DidNotReceiveWithAnyArgs().Publish<TriggerOrderReleaseNotification>(default!);

    }

    [Fact]
    public void Release_ShouldPublishNotification_WhenOrderIsNotNull() {

        // Arrange
        var order = new OrderBuilder().Buid();
        _sut.ReplaceOrder(order);

        var profile = ReleaseProfile.Default;
        var company = new Company(Guid.Empty, "", new(), "", "", "", "", profile, CompleteProfile.Default);
        var query = new GetCompanyById.Query(order.VendorId);
        var response = new Response<Company?>(company);
        _bus.Send(query).Returns(response);

        // Act
        _sut.Release().Wait();

        // Assert
        _sut.Order.Should().NotBeNull();
        _bus.Received(1).Send(query);
        _bus.Received(1).Publish(new TriggerOrderReleaseNotification(order, profile));

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
