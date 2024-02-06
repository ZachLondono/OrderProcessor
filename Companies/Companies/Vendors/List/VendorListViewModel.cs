using Domain.Infrastructure.Bus;

namespace Companies.Vendors.List;

public class VendorListViewModel {

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

    private IEnumerable<VendorListItem> _vendors = Enumerable.Empty<VendorListItem>();
    public IEnumerable<VendorListItem> Vendors {
        get => _vendors;
        set {
            _vendors = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private readonly IBus _bus;

    public VendorListViewModel(IBus bus) {
        _bus = bus;
    }

    public async Task LoadVendors() {

        Error = null;
        IsLoading = true;

        try {

            var response = await _bus.Send(new GetAllVendors.Query());

            response.Match(
                vendors => Vendors = vendors,
                error => Error = error
            );

        } catch (Exception ex) {

            Error = new() {
                Title = $"Exception was thrown while loading vendor list",
                Details = $"{ex.Message}<br><br>{ex.StackTrace}"
            };

        }

        IsLoading = false;

    }

}
