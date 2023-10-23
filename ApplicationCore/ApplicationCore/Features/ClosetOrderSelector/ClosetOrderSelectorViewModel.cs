using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Features.ClosetOrderSelector;

public class ClosetOrderSelectorViewModel {

    private readonly IBus _bus;

    public Action? OnPropertyChanged { get; set; }

    private List<ClosetOrder> _openClosetOrders = new();
    public List<ClosetOrder> OpenClosetOrders {
        get => _openClosetOrders;
        set {
            _openClosetOrders = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private bool _isLoading = false; 
    public bool IsLoading {
        get => _isLoading;
        set {
            _isLoading = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private string _error = ""; 
    public string Error {
        get => _error;
        set {
            _error = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public ClosetOrderSelectorViewModel(IBus bus) {
        _bus = bus;
    }

    public async Task LoadOpenOrders() {

        IsLoading = true;

        var response = await _bus.Send(new GetOpenClosetOrders.Query());
        response.Match(
            orders => _openClosetOrders = new(orders),
            error => _error = $"{error.Title} - {error.Details}"
        );
        _isLoading = false;

        IsLoading = false;

    }

}
