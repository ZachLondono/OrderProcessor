using ApplicationCore.Infrastructure.Bus;

namespace Domain.Companies.Customers.List;

internal class CustomerListViewModel {

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

    public CustomerListViewModel(IBus bus) {
        _bus = bus;
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
                Title = $"Exception was thrown while loading customer list",
                Details = $"{ex.Message}<br><br>{ex.StackTrace}"
            };

        }

        IsLoading = false;

    }

}
