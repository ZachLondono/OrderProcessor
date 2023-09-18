using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using Microsoft.Extensions.Logging;
using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Features.Orders.Shared.State;

public class OrderState {

    public Order? Order { get; private set; }
    public bool IsDirty { get; private set; }

    private readonly IBus _bus;
    private readonly ILogger<OrderState> _logger;

    public OrderState(IBus bus, ILogger<OrderState> logger) {
        _bus = bus;
        _logger = logger;
    }

    public async Task<bool> LoadOrder(Guid orderId) {
        var result = await _bus.Send(new GetOrderById.Query(orderId));
        return result.Match(
            order => {
                Order = order;
                _logger.LogInformation("Current order updated to order: {OrderId}", orderId);
                return true;
            },
            error => {
                _logger.LogError("Error loading order while trying to set current order {Error}", error);
                return false;
            }
        );
    }

    public void ReplaceOrder(Order order) {
        Order = order;
        _logger.LogInformation("Current order replaced with order: {OrderId}", order.Id);
    }

    public void SetDueDate(DateTime? dueDate) {
        if (Order is null) return;
        Order.DueDate = dueDate;
        IsDirty = true;
    }

    public void SetNote(string note) {
        if (Order is null) return;
        Order.Note = note;
        IsDirty = true;
    }

    public void SetWorkingDirectory(string workingDirectory) {
        if (Order is null) return;
        Order.WorkingDirectory = workingDirectory;
        IsDirty = true;
    }

    public async Task SaveChanges() {
        if (Order is null || !IsDirty) return;
        await _bus.Send(new SaveChanges.Command(Order));
        IsDirty = false;
    }

}
