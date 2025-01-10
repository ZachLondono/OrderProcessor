using Domain.Infrastructure.Bus;
using Microsoft.Extensions.Logging;

namespace Companies.Customers.List;

public class CustomerListViewModel {

    public Action? OnPropertyChanged { get; set; }

    private bool _isLoading = false;
    public bool IsLoading {
        get => _isLoading;
        set {
            _isLoading = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private Error? _error = null;
    public Error? Error {
        get => _error;
        set {
            _error = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private IEnumerable<CustomerListItem> _customers = Enumerable.Empty<CustomerListItem>();
    public IEnumerable<CustomerListItem> Customers {
        get => _customers;
        set {
            _customers = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private readonly IBus _bus;
    private ILogger<CustomerListViewModel> _logger;

    public CustomerListViewModel(IBus bus, ILogger<CustomerListViewModel> logger) {
        _bus = bus;
        _logger = logger;
    }

    public async Task LoadCustomers() {

        Error = null;
        IsLoading = true;

        try {

            var response = await _bus.Send(new GetAllCustomers.Query());

            response.Match(
                customers => Customers = customers,
                error => Error = error
            );

        } catch (Exception ex) {

            Error = new() {
                Title = $"Error Loading Customer List.",
                Details = ex.Message
            };

            _logger.LogError(ex, "Exception thrown while trying to load customer list");

        }

        IsLoading = false;

    }

}
