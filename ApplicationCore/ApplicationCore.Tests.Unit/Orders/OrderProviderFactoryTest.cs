using ApplicationCore.Features.Orders.Loader;
using ApplicationCore.Features.Orders.Loader.Providers;
using ApplicationCore.Features.Orders.Providers;
using FluentAssertions;
using NSubstitute;

namespace ApplicationCore.Tests.Unit.Orders;

public class OrderProviderFactoryTest {

    private readonly OrderProviderFactory _sut;
    private readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();

    public OrderProviderFactoryTest() {

        _sut = new(_serviceProvider);

    }

    [Fact]
    public void Should_NotThrowException_ForAnySourceType() {

        var provider = Substitute.For<IOrderProvider>();

        foreach (var sourceType in Enum.GetValues(typeof(OrderSourceType))) {

            // Act
            var action = () => _sut.GetOrderProvider((OrderSourceType)sourceType);

            // Assert
            action.Should().NotThrow<KeyNotFoundException>();

        }

    }

}