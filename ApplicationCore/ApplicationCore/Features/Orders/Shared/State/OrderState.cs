using ApplicationCore.Infrastructure;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Orders.Shared.State;

public class OrderState {

    public Order? Order { get; private set; }

    private readonly IBus _bus;
    private readonly ILogger<OrderState> _logger;

    public OrderState(IBus bus, ILogger<OrderState> logger) {
        _bus = bus;
        _logger = logger;
    }

    public async Task LoadOrder(Guid orderId) {
        var result = await _bus.Send(new GetOrderById.Query(orderId));
        result.Match(
            order => {
                Order = order;
                _logger.LogInformation("Current order updated to order: {OrderId}", orderId);
            },
            error => {
                _logger.LogError("Error loading order while trying to set current order {Error}", error);
            }
        );
    }

    public void ReplaceOrder(Order order) {
        Order = order;
        _logger.LogInformation("Current order replaced with order: {OrderId}", order.Id);
    }

}
