using Domain.Infrastructure.Bus;

namespace ApplicationCore.Features.FivePieceOrderRelease;

public class FivePieceDoorOrderFileSelectorViewModel {

    private readonly IBus _bus;

    public Action? OnPropertyChanged { get; set; }

    private List<FivePieceOrderFile> _openOrders = [];
    public List<FivePieceOrderFile> OpenOrders {
        get => _openOrders;
        set {
            _openOrders = value;
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

    public FivePieceDoorOrderFileSelectorViewModel(IBus bus) {
        _bus = bus;
    }

    public async Task LoadOpenOrders() {

        IsLoading = true;

        var response = await _bus.Send(new GetOpenFivePieceOrders.Query());
        response.Match(
            orders => _openOrders = new(orders),
            error => _error = $"{error.Title} - {error.Details}"
        );

        IsLoading = false;

    }

}