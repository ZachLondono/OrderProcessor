using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Release;
using ApplicationCore.Infrastructure;
using ApplicationCore.Features.Orders.Complete;
using ApplicationCore.Features.Companies.Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Shared.State;

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
        Order = new Order(Order.Id, Order.Source, Order.Status, number, name, Order.CustomerId, Order.VendorId, productionNote, Order.CustomerComment, Order.OrderDate, Order.ReleaseDate, Order.ProductionDate, Order.CompleteDate, Order.Shipping, Order.Tax, Order.PriceAdjustment, Order.Rush, Order.Info, Order.Products, Order.AdditionalItems);
        IsDirty = true;
    }

    public void UpdateCustomer(Guid customerId) {
        if (Order is null) return;
        Order = new Order(Order.Id, Order.Source, Order.Status, Order.Number, Order.Name, customerId, Order.VendorId, Order.ProductionNote, Order.CustomerComment, Order.OrderDate, Order.ReleaseDate, Order.ProductionDate, Order.CompleteDate, Order.Shipping, Order.Tax, Order.PriceAdjustment, Order.Rush, Order.Info, Order.Products, Order.AdditionalItems);
        IsDirty = true;
    }

    public void UpdateVendor(Guid vendorId) {
        if (Order is null) return;
        Order = new Order(Order.Id, Order.Source, Order.Status, Order.Number, Order.Name, Order.CustomerId, vendorId, Order.ProductionNote, Order.CustomerComment, Order.OrderDate, Order.ReleaseDate, Order.ProductionDate, Order.CompleteDate, Order.Shipping, Order.Tax, Order.PriceAdjustment, Order.Rush, Order.Info, Order.Products, Order.AdditionalItems);
        IsDirty = true;
    }

    public void ScheduleProduction(DateTime productionDate) {
        if (Order is null) return;
        Order = new Order(Order.Id, Order.Source, Order.Status, Order.Number, Order.Name, Order.CustomerId, Order.VendorId, Order.ProductionNote, Order.CustomerComment, Order.OrderDate, Order.ReleaseDate, productionDate, Order.CompleteDate, Order.Shipping, Order.Tax, Order.PriceAdjustment, Order.Rush, Order.Info, Order.Products, Order.AdditionalItems);
        IsDirty = true;
    }

    public async Task<Response> SaveChanges() {
        if (Order is null) return new(new() {
            Title = "No Order to Save",
            Details = "There is no order set that can be saved"
        });

        var response = await _bus.Send(new UpdateOrder.Command(Order));
        IsDirty = false;

        if (response.IsError) {

            string details = "";
            response.OnError(e => details = e.Details);

            return new(new() {
                Title = "Error saving order changes",
                Details = details
            });

        }
        return new();

    }

    public async Task<Response> Complete() {
        if (Order is null) return new(new() {
            Title = "No Order to Complete",
            Details = "There is no order set that can be completed"
        });

        var response = await _bus.Send(new GetCompanyById.Query(Order.VendorId));

        Error? error = null;
        response.Match(
            async company => {

                try {

                    await _bus.Publish(new TriggerOrderCompleteNotification(Order, company.CompleteProfile));
                    Order.Complete();
                    IsDirty = true;

                } catch (Exception ex) {
                    error = new Error() {
                        Title = "Error Completing Order",
                        Details = ex.ToString()
                    };
                }

            },
            error => {
                error = new Error() {
                    Title = "Error Completing Order",
                    Details = "Could not load completion settings. " + error.Details
                };
            }
        );

        if (error is not null) return new(error);
        return new();
    }

    public async Task Release(ReleaseProfile? profile = null) {
        if (Order is null) return;

        ReleaseProfile? releaseProfile = profile;

        if (releaseProfile is null) {
            var response = await _bus.Send(new GetCompanyById.Query(Order.VendorId));
            response.Match(
                c => releaseProfile = c?.ReleapseProfile ?? null,
                error => {
                    // TODO: notify and log error
                }
            );
        }

        if (releaseProfile is null) return; // TODO: notify of error

        await ReleaseWithProfile(releaseProfile);

    }

    private async Task ReleaseWithProfile(ReleaseProfile profile) {
        if (Order is null) return;
        await _bus.Publish(new TriggerOrderReleaseNotification(Order, profile));
        Order.Release();
        _uibus.Publish(new OrderReleaseCompletedNotification());
        IsDirty = true;
    }

    // TODO: add method to add price adjustments to order

    // TODO: add method to add additional items to order

    // TODO: add method to add info fields to order

    public void ReplaceOrder(Order order) {
        Order = order;
        IsDirty = false;
    }

}
