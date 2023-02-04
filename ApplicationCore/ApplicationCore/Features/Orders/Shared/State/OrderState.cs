using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.Orders.Release;
using ApplicationCore.Infrastructure;
using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;

namespace ApplicationCore.Features.Orders.Shared.State;

public class OrderState {

    public Order? Order { get; private set; }

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
            },
            error => {
                // TODO: log error
            }
        );
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
        _uibus.Publish(new OrderReleaseCompletedNotification());
    }

    public void ReplaceOrder(Order order) {
        Order = order;
    }

}
