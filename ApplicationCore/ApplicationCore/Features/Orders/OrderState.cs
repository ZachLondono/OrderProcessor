using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.Orders.Commands;
using ApplicationCore.Features.Orders.Domain;
using ApplicationCore.Features.Orders.Release;
using ApplicationCore.Features.Orders.Queries;
using ApplicationCore.Infrastructure;
using ApplicationCore.Features.Orders.Complete;

namespace ApplicationCore.Features.Orders;

public class OrderState {

    public Order? Order { get; private set; }
    public bool IsDirty { get; private set; } = false;

    private readonly IBus _bus;
    private readonly IUIBus _uibus;

    public OrderState(IBus bus, IUIBus uibus) {
        _bus = bus;
        _uibus = uibus;
    }

    public async Task LoadOrder(Guid orderId) {
        var result = await _bus.Send(new GetOrderById.Query(orderId));
        result.Match(
            order => {
                Order = order;
                IsDirty = false;
            },
            error => {
                // TODO: log error
            }
        );
    }

    public void UpdateInfo(string number, string name, string productionNote) {
        if (Order is null) return;
        Order = new Order(Order.Id, Order.Status, number, name, Order.CustomerId, Order.VendorId, productionNote, Order.CustomerComment, Order.OrderDate, Order.ReleaseDate, Order.ProductionDate, Order.CompleteDate, Order.Tax, Order.Shipping, Order.PriceAdjustment, Order.Info, Order.Boxes, Order.AdditionalItems);
        IsDirty = true;
    }

    public void UpdateCustomer(Guid customerId) {
        if (Order is null) return;
        Order = new Order(Order.Id, Order.Status, Order.Number, Order.Name, customerId, Order.VendorId, Order.ProductionNote, Order.CustomerComment, Order.OrderDate, Order.ReleaseDate, Order.ProductionDate, Order.CompleteDate, Order.Tax, Order.Shipping, Order.PriceAdjustment, Order.Info, Order.Boxes, Order.AdditionalItems);
        IsDirty = true;
    }

    public void UpdateVendor(Guid vendorId) {
        if (Order is null) return;
        Order = new Order(Order.Id, Order.Status, Order.Number, Order.Name, Order.CustomerId, vendorId, Order.ProductionNote, Order.CustomerComment, Order.OrderDate, Order.ReleaseDate, Order.ProductionDate, Order.CompleteDate, Order.Tax, Order.Shipping, Order.PriceAdjustment, Order.Info, Order.Boxes, Order.AdditionalItems);
        IsDirty = true;
    }

    public async Task SaveChanges() {
        if (Order is null) return;
        await _bus.Send(new UpdateOrder.Command(Order));
        IsDirty = false;
    }

    public async Task Complete() {
        if (Order is null) return;
        var response = await _bus.Send(new GetCompleteProfileByVendorId.Query(Order.VendorId));

        response.Match(
            async profile => {
                await _bus.Publish(new TriggerOrderCompleteNotification(Order, profile));
                Order.Complete();
                IsDirty = true;
            },
            error => {
                // TODO: notify and log error
            }
        );

    }

    public async Task Release() {
        if (Order is null) return;
        var response = await _bus.Send(new GetReleaseProfileByVendorId.Query(Order.VendorId));

        response.Match(
            async profile => {
                await _bus.Publish(new TriggerOrderReleaseNotification(Order, profile));
                Order.Release();
                _uibus.Publish(new OrderReleaseCompletedNotification());
                IsDirty = true;
            },
            error => {
                // TODO: notify and log error
            }
        );
    }

    // TODO: add method to add price adjustments to order

    // TODO: add method to add additional items to order

    // TODO: add method to add info fields to order

    public void ReplaceOrder(Order order) {
        Order = order;
        IsDirty = false;
    }

}
