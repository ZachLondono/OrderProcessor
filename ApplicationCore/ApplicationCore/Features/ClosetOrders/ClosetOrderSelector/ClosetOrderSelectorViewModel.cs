using Domain.Infrastructure.Bus;

namespace ApplicationCore.Features.ClosetOrders.ClosetOrderSelector;

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

    private bool _isLoading = true;
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
        _error = string.Empty;

        var response = await _bus.Send(new GetOpenClosetOrders.Query());
        response.Match(
            orders => _openClosetOrders = new(orders),
            error => {
                _openClosetOrders = [];
                _error = $"{error.Title} - {error.Details}";
            }
        );

        IsLoading = false;

    }

}
