using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Features.OrderRelationshipList;

internal class OrderRelationshipListViewModel {

    private readonly IBus _bus;

    public Action? OnPropertyChanged { get; set; }

    private List<RelatedOrder> _relatedOrders = new();
    public List<RelatedOrder> RelatedOrders {
        get => _relatedOrders;
        set {
            _relatedOrders = new(value);
            OnPropertyChanged?.Invoke();
        }
    }

    private string? _error = null;
    public string? Error {
        get => _error;
        set {
            _error = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public OrderRelationshipListViewModel(IBus bus) {
        _bus = bus;
    }

    public async Task LoadRelatedOrders(Guid orderId) {

        Error = null;

        try {

            var response = await _bus.Send(new GetRelatedOrders.Query(orderId));

            response.OnSuccess(orders => RelatedOrders = new(orders));

        } catch {

            Error = "Failed to load related orders";

        }

    }

}
