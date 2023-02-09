using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.Orders.Release;
using ApplicationCore.Infrastructure;
using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Orders.Shared.State;

public class OrderState {

    public Order? Order { get; private set; }

    private readonly IBus _bus;
    private readonly IUIBus _uibus;
    private readonly ILogger<OrderState> _logger;

    public OrderState(IBus bus, IUIBus uibus, ILogger<OrderState> logger) {
        _bus = bus;
        _uibus = uibus;
        _logger = logger;
    }

    public async Task LoadOrder(Guid orderId) {
        var result = await _bus.Send(new GetOrderById.Query(orderId));
        result.Match(
            order => {
                Order = order;
            },
            error => {
                _logger.LogError("Error loading order while trying to set current order {Error}", error);
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
                    _logger.LogError("Error loading release profile for order's vendor while trying to release order {Error}", error);
                }
            );
        }

        if (releaseProfile is null) {
            _uibus.Publish(new OrderReleaseErrorNotification("No release profile set"));
            return;
        }

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
