using Domain.Companies.Entities;
using Domain.Companies.Customers.Commands;
using ApplicationCore.Infrastructure.Bus;

namespace Domain.Companies.Customers.Create;

internal class CreateCustomerViewModel {

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

    public NewCustomerModel Model { get; } = new();
    private readonly IBus _bus;

    public CreateCustomerViewModel(IBus bus) {
        _bus = bus;
    }

    public async Task<Guid> Submit() {

        Error = null;
        IsLoading = true;

        Guid newCustomerId = Guid.Empty;

        try {

            var customer = Customer.Create(Model.Name ?? "", Model.ShippingMethod, Model.ShippingContact, Model.ShippingAddress, Model.BillingContact, Model.BillingAddress);

            var response = await _bus.Send(new InsertCustomer.Command(customer, Model.AllmoxyId));

            response.OnError(error => Error = error);

            newCustomerId = customer.Id;

        } catch (Exception ex) {

            _error = new() {
                Title = $"Exception occurred while trying to insert new customer in database",
                Details = $"{ex.Message}<br><br>{ex.StackTrace}"
            };

        }

        IsLoading = false;

        return newCustomerId;

    }

}
