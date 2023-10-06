using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using Microsoft.Extensions.Logging;
using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Features.Orders.Shared.State;

public class OrderState {

    public Order? Order { get; private set; }
    public bool IsNoteDirty { get; private set; }
    public bool IsWorkingDirectoryDirty { get; private set; }
    public bool IsDueDateDirty { get; private set; }

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
        IsDueDateDirty = true;
    }

    public void SetNote(string note) {
        if (Order is null) return;
        Order.Note = note;
        IsNoteDirty = true;
    }

    public void SetWorkingDirectory(string workingDirectory) {
        if (Order is null) return;
        Order.WorkingDirectory = workingDirectory;
        IsWorkingDirectoryDirty = true;
    }

    public async Task SaveNote() {
        if (Order is null || !IsNoteDirty) return;
        await _bus.Send(new UpdateOrderNote.Command(Order.Id, Order.Note));
        IsNoteDirty = false;
    }

    public async Task SaveWorkingDirectory() {
        if (Order is null || !IsWorkingDirectoryDirty) return;
        await _bus.Send(new UpdateOrderWorkingDirectory.Command(Order.Id, Order.WorkingDirectory));
        IsWorkingDirectoryDirty = false;
    }

    public async Task SaveDueDate() {
        if (Order is null || !IsDueDateDirty) return;
        await _bus.Send(new UpdateOrderDueDate.Command(Order.Id, Order.DueDate));
        IsDueDateDirty = false;
    }

}
