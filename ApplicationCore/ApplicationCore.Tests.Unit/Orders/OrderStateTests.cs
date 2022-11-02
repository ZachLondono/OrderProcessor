using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.Orders;
using ApplicationCore.Features.Orders.Commands;
using ApplicationCore.Features.Orders.Domain;
using ApplicationCore.Features.Orders.Release;
using ApplicationCore.Features.Orders.Queries;
using ApplicationCore.Infrastructure;
using FluentAssertions;
using NSubstitute;

namespace ApplicationCore.Tests.Unit.Orders;

public class OrderStateTests {

    private readonly OrderState _sut;
    private readonly IBus _bus = Substitute.For<IBus>();

    public OrderStateTests() {
        _sut = new(_bus);
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
        var order = new Order(Guid.NewGuid(), "", "", Guid.NewGuid(), Guid.NewGuid(), "", DateTime.Now, 0M, 0M, new Dictionary<string, string>(), new List<DrawerBox>(), new List<AdditionalItem>());

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

        // Act
        _sut.UpdateInfo(newNumber, newName);

        // Assert
        _sut.IsDirty.Should().BeFalse();
        _sut.Order.Should().BeNull();

    }

    [Fact]
    public void UpdateInfo_ShouldReplaceOrder_WhenOrderIsNotNull() {

        // Arrange
        string newNumber = "Number";
        string newName = "Name";
        var order = new Order(Guid.NewGuid(), "", "", Guid.NewGuid(), Guid.NewGuid(), "", DateTime.Now, 0M, 0M, new Dictionary<string, string>(), new List<DrawerBox>(), new List<AdditionalItem>());
        _sut.ReplaceOrder(order);

        // Act
        _sut.UpdateInfo(newNumber, newName);

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
        var order = new Order(Guid.NewGuid(), "", "", Guid.NewGuid(), Guid.NewGuid(), "", DateTime.Now, 0M, 0M, new Dictionary<string, string>(), new List<DrawerBox>(), new List<AdditionalItem>());
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
        var order = new Order(Guid.NewGuid(), "", "", Guid.NewGuid(), Guid.NewGuid(), "", DateTime.Now, 0M, 0M, new Dictionary<string, string>(), new List<DrawerBox>(), new List<AdditionalItem>());
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
        _sut.SaveChanges().Wait();

        // Assert
        _sut.IsDirty.Should().BeFalse();
        _sut.Order.Should().BeNull();
        _bus.DidNotReceiveWithAnyArgs().Send<ReleaseProfile>(default!);
        _bus.DidNotReceiveWithAnyArgs().Publish<TriggerOrderReleaseNotification>(default!);

    }

    [Fact]
    public void SaveChanges_ShouldCallUpdate_AndResetIsDirty_WhenOrderIsNotNull() {

        // Arrange
        var order = new Order(Guid.NewGuid(), "", "", Guid.NewGuid(), Guid.NewGuid(), "", DateTime.Now, 0M, 0M, new Dictionary<string, string>(), new List<DrawerBox>(), new List<AdditionalItem>());
        _sut.ReplaceOrder(order);
        Guid vendorId = Guid.NewGuid();
        _sut.UpdateVendor(vendorId);

        // Act
        _sut.SaveChanges().Wait();

        // Assert
        _sut.IsDirty.Should().BeFalse();
        _sut.Order.Should().NotBeNull();
        _bus.Received(1).Send(new UpdateOrder.Command(_sut.Order!));
        _bus.DidNotReceiveWithAnyArgs().Publish<TriggerOrderReleaseNotification>(default!);

    }

    [Fact]
    public void Release_ShouldDoNothing_WhenOrderIsNull() {

        // Act
        _sut.Release().Wait();

        // Assert
        _sut.IsDirty.Should().BeFalse();
        _sut.Order.Should().BeNull();
        _bus.DidNotReceiveWithAnyArgs().Send<ReleaseProfile>(default!);
        _bus.DidNotReceiveWithAnyArgs().Publish<TriggerOrderReleaseNotification>(default!);

    }

    [Fact]
    public void Release_ShouldPublishNotification_AndSetIsDirty_WhenOrderIsNotNull() {

        // Arrange
        var order = new Order(Guid.NewGuid(), "", "", Guid.NewGuid(), Guid.NewGuid(), "", DateTime.Now, 0M, 0M, new Dictionary<string, string>(), new List<DrawerBox>(), new List<AdditionalItem>());
        _sut.ReplaceOrder(order);

        var profile = new ReleaseProfile();
        var query = new GetReleaseProfileByVendorId.Query(order.VendorId);
        _bus.Send(query).Returns(profile);

        // Act
        _sut.Release().Wait();

        // Assert
        _sut.IsDirty.Should().BeTrue();
        _sut.Order.Should().NotBeNull();
        _bus.Received(1).Send(query);
        _bus.Received(1).Publish(new TriggerOrderReleaseNotification(profile));

    }

    [Fact]
    public void LoadOrder_ShouldCallGetOrder_AndResetIsDirty_WhenOrderIsNotNull() {

        // Arrange
        var order = new Order(Guid.NewGuid(), "", "", Guid.NewGuid(), Guid.NewGuid(), "", DateTime.Now, 0M, 0M, new Dictionary<string, string>(), new List<DrawerBox>(), new List<AdditionalItem>());
        var query = new GetOrderById.Query(order.Id);
        _bus.Send(query).Returns(order);

        // Act
        _sut.LoadOrder(order.Id).Wait();

        // Assert
        _sut.IsDirty.Should().BeFalse();
        _sut.Order.Should().NotBeNull();
        _sut.Order.Should().Be(order);
        _bus.Received(1).Send(query);

    }


}
