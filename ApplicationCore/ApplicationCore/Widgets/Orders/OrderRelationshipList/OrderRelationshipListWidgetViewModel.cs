using ApplicationCore.Features.Orders.OrderRelationships;
using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Widgets.Orders.OrderRelationshipList;

internal class OrderRelationshipListWidgetViewModel {

    private readonly IBus _bus;
    private List<RelatedOrder> _relatedOrders = new();

    public Action? OnPropertyChanged { get; set; }

    public List<RelatedOrder> RelatedOrders {
        get => _relatedOrders;
        set {
            _relatedOrders = new(value);
            OnPropertyChanged?.Invoke();
        }
    }

    public OrderRelationshipListWidgetViewModel(IBus bus) {
        _bus = bus;
    }

    public async Task LoadRelatedOrders(Guid orderId) {

        var response = await _bus.Send(new GetRelatedOrders.Query(orderId));

        response.OnSuccess(orders => RelatedOrders = new(orders));

    }

}
