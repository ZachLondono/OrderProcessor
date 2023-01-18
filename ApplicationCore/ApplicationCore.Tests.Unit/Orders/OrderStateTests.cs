using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Release;
using ApplicationCore.Infrastructure;
using FluentAssertions;
using NSubstitute;
using ApplicationCore.Features.Orders.Shared.State;
using ApplicationCore.Features.Companies.Domain;

namespace ApplicationCore.Tests.Unit.Orders;

public class OrderStateTests {

    private readonly OrderState _sut;
    private readonly IBus _bus = Substitute.For<IBus>();
    private readonly IUIBus _uiBus = Substitute.For<IUIBus>();

    public OrderStateTests() {
        _sut = new(_bus, _uiBus);
    }

    [Fact]
    public void ShouldBeNullByDefault() {

        // Assert
        _sut.Order.Should().BeNull();
        _sut.IsDirty.Should().BeFalse();

    }

    [Fact]
    public void ReplaceOrder_ShouldSetOrder_AndResetIsDirty() {

        // Arrange
        var order = new OrderBuilder().Buid();

        // Act
        _sut.ReplaceOrder(order);

        // Assert
        _sut.Order.Should().Be(order);
        _sut.IsDirty.Should().BeFalse();

    }

    [Fact]
    public void UpdateInfo_ShouldDoNothing_WhenOrderIsNull() {

        // Arrange
        string newNumber = "Number";
        string newName = "Name";
        string note = "Production Note";

        // Act
        _sut.UpdateInfo(newNumber, newName, note);

        // Assert
        _sut.IsDirty.Should().BeFalse();
        _sut.Order.Should().BeNull();

    }

    [Fact]
    public void UpdateInfo_ShouldReplaceOrder_WhenOrderIsNotNull() {

        // Arrange
        string newNumber = "Number";
        string newName = "Name";
        string note = "Production Note";
        var order = new OrderBuilder().Buid();
        _sut.ReplaceOrder(order);

        // Act
        _sut.UpdateInfo(newNumber, newName, note);

        // Assert
        _sut.IsDirty.Should().BeTrue();
        _sut.Order.Should().NotBe(order);
        _sut.Order.Should().NotBeNull();
        _sut.Order!.Number.Should().Be(newNumber);
        _sut.Order.Name.Should().Be(newName);

    }

    [Fact]
    public void UpdateCustomer_ShouldDoNothing_WhenOrderIsNull() {

        // Arrange
        Guid customerId = Guid.NewGuid();

        // Act
        _sut.UpdateCustomer(customerId);

        // Assert
        _sut.IsDirty.Should().BeFalse();
        _sut.Order.Should().BeNull();

    }

    [Fact]
    public void UpdateCustomer_ShouldReplaceOrder_WhenOrderIsNotNull() {

        // Arrange
        var order = new OrderBuilder().Buid();
        _sut.ReplaceOrder(order);
        Guid customerId = Guid.NewGuid();

        // Act
        _sut.UpdateCustomer(customerId);


        // Assert
        _sut.IsDirty.Should().BeTrue();
        _sut.Order.Should().NotBe(order);
        _sut.Order.Should().NotBeNull();
        _sut.Order!.CustomerId.Should().Be(customerId);

    }

    [Fact]
    public void UpdateVendor_ShouldDoNothing_WhenOrderIsNull() {

        // Arrange
        Guid vendorId = Guid.NewGuid();

        // Act
        _sut.UpdateVendor(vendorId);

        // Assert
        _sut.IsDirty.Should().BeFalse();
        _sut.Order.Should().BeNull();

    }

    [Fact]
    public void UpdateVendor_ShouldReplaceOrder_WhenOrderIsNotNull() {

        // Arrange
        var order = new OrderBuilder().Buid();
        _sut.ReplaceOrder(order);
        Guid vendorId = Guid.NewGuid();

        // Act
        _sut.UpdateVendor(vendorId);


        // Assert
        _sut.IsDirty.Should().BeTrue();
        _sut.Order.Should().NotBe(order);
        _sut.Order.Should().NotBeNull();
        _sut.Order!.VendorId.Should().Be(vendorId);

    }

    [Fact]
    public void SaveChanges_ShouldDoNothing_WhenOrderIsNull() {

        // Act
        var result = _sut.SaveChanges().Result;

        // Assert
        result.IsError.Should().BeTrue();
        _sut.IsDirty.Should().BeFalse();
        _sut.Order.Should().BeNull();
        _bus.DidNotReceiveWithAnyArgs().Send(default(IQuery<ReleaseProfile>)!);
        _bus.DidNotReceiveWithAnyArgs().Send(default(ICommand<ReleaseProfile>)!);
        _bus.DidNotReceiveWithAnyArgs().Publish<TriggerOrderReleaseNotification>(default!);

    }

    [Fact]
    public void SaveChanges_ShouldCallUpdate_AndResetIsDirty_WhenOrderIsNotNull() {

        // Arrange
        var order = new OrderBuilder().Buid();
        _sut.ReplaceOrder(order);
        Guid vendorId = Guid.NewGuid();
        _sut.UpdateVendor(vendorId);
        _bus.Send(new UpdateOrder.Command(order)).ReturnsForAnyArgs(new Response());

        // Act
        var result = _sut.SaveChanges().Result;

        // Assert
        result.IsError.Should().BeFalse();
        _sut.IsDirty.Should().BeFalse();
        _sut.Order.Should().NotBeNull();
        _bus.Received(1).Send(new UpdateOrder.Command(_sut.Order!));

    }

    [Fact]
    public void SaveChanges_ShouldReturnError_WhenUpdateFails() {

        // Arrange
        var order = new OrderBuilder().Buid();
        _sut.ReplaceOrder(order);
        Guid vendorId = Guid.NewGuid();
        _sut.UpdateVendor(vendorId);
        _bus.Send(new UpdateOrder.Command(order)).ReturnsForAnyArgs(new Response(new Error() { Title = "Error", Details = "Error details" }));

        // Act
        var result = _sut.SaveChanges().Result;

        // Assert
        result.IsError.Should().BeTrue();
        _sut.Order.Should().NotBeNull();
        _bus.Received(1).Send(new UpdateOrder.Command(_sut.Order!));

    }

    [Fact]
    public void Release_ShouldDoNothing_WhenOrderIsNull() {

        // Act
        _sut.Release().Wait();

        // Assert
        _sut.IsDirty.Should().BeFalse();
        _sut.Order.Should().BeNull();
        _bus.DidNotReceiveWithAnyArgs().Send(default(IQuery<ReleaseProfile>)!);
        _bus.DidNotReceiveWithAnyArgs().Send(default(ICommand<ReleaseProfile>)!);
        _bus.DidNotReceiveWithAnyArgs().Publish<TriggerOrderReleaseNotification>(default!);

    }

    [Fact]
    public void Release_ShouldPublishNotification_AndSetIsDirty_WhenOrderIsNotNull() {

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
        _sut.IsDirty.Should().BeTrue();
        _sut.Order.Should().NotBeNull();
        _bus.Received(1).Send(query);
        _bus.Received(1).Publish(new TriggerOrderReleaseNotification(order, profile));

    }

    [Fact]
    public void LoadOrder_ShouldCallGetOrder_AndResetIsDirty_WhenOrderIsNotNull() {

        // Arrange
        var order = new OrderBuilder().Buid();
        var query = new GetOrderById.Query(order.Id);
        var response = new Response<Order>(order);
        _bus.Send(query).Returns(response);

        // Act
        _sut.LoadOrder(order.Id).Wait();

        // Assert
        _sut.IsDirty.Should().BeFalse();
        _sut.Order.Should().NotBeNull();
        _sut.Order.Should().Be(order);
        _bus.Received(1).Send(query);

    }

    [Fact]
    public void ScheduleProduction_ShouldSetProductionDateAndSetDirty() {

        // Arrange
        var order = new OrderBuilder().Buid();
        _sut.ReplaceOrder(order);
        var productionDate = DateTime.Today.AddDays(5);

        // Act
        _sut.ScheduleProduction(productionDate);

        // Assert
        _sut.IsDirty.Should().BeTrue();
        _sut.Order.Should().NotBeNull();
        _sut.Order!.ProductionDate.Should().Be(productionDate);

    }

    [Fact]
    public void SheduleProduction_ShouldSetProductionDateAndSetDirty() {

        // Arrange
        var order = new OrderBuilder().Buid();
        _sut.ReplaceOrder(order);
        var productionDate = DateTime.Today.AddDays(5);

        // Act
        _sut.ScheduleProduction(productionDate);

        // Assert
        _sut.IsDirty.Should().BeTrue();
        _sut.Order.Should().NotBeNull();
        _sut.Order!.ProductionDate.Should().Be(productionDate);

    }

}
