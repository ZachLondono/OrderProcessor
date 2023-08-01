using ApplicationCore.Features.Orders.OrderRelationships;
using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Widgets.Orders.OrderRelationshipList;

internal class OrderRelationshipListWidgetViewModel {

    private readonly IBus _bus;
    private IEnumerable<RelatedOrder> _relatedOrders = Enumerable.Empty<RelatedOrder>();

    public Action? OnPropertyChanged { get; set; }

    public IEnumerable<RelatedOrder> RelatedOrders {
        get => _relatedOrders;
        set {
            _relatedOrders = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public OrderRelationshipListWidgetViewModel(IBus bus) {
        _bus = bus;
    }

    public async Task LoadRelatedOrders(Guid orderId) {

        var response = await _bus.Send(new GetRelatedOrders.Query(orderId));

        response.OnSuccess(orders => RelatedOrders = orders);

    }

}
