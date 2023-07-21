using ApplicationCore.Features.Companies.Contracts.Entities;
using ApplicationCore.Features.Companies.Customers.Commands;
using ApplicationCore.Features.Companies.Customers.Queries;
using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Features.Companies.Customers.Edit;

internal class EditCustomerViewModel {

    public Action? OnPropertyChanged { get; set; }

    private bool _isLoading = false;
    public bool IsLoading {
        get => _isLoading;
        set {
            _isLoading = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private bool _isSaving = false;
    public bool IsSaving {
        get => _isSaving;
        set {
            _isSaving = value;
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

    public ExistingCustomerModel? Model { get; private set; } = null;
    private readonly IBus _bus;

    public EditCustomerViewModel(IBus bus) {
        _bus = bus;
    }

    public async Task SetCustomerId(Guid customerId) {

        Error = null;
        IsLoading = true;

        var response = await _bus.Send(new GetCustomerById.Query(customerId));

        Model = response.Match<ExistingCustomerModel?>(

            customer => customer is null ? null : new() {
                Id = customerId,
                Name = customer.Name,
                OrderNumberPrefix = customer.OrderNumberPrefix,
                BillingAddress = customer.BillingAddress,
                BillingContact = customer.BillingContact,
                ShippingMethod = customer.ShippingMethod,
                ShippingAddress = customer.ShippingAddress,
                ShippingContact = customer.ShippingContact,
                ClosetProSettings = customer.ClosetProSettings
            },
            error => null
        );

        IsLoading = false;

    }

    public async Task Submit() {

        if (Model is null) {
            Error = new() {
                Title = "Cannot Save",
                Details = "Customer is not set"
            };
            return;
        }

        Error = null;
        IsSaving = true;

        try {

            var customer = new Customer(Model.Id, Model.Name ?? "", Model.ShippingMethod, Model.ShippingContact, Model.ShippingAddress, Model.BillingContact, Model.BillingAddress, Model.OrderNumberPrefix, Model.ClosetProSettings);

            var response = await _bus.Send(new UpdateCustomer.Command(customer));

            response.OnError(error => Error = error);

        } catch (Exception ex) {

            _error = new() {
                Title = $"Exception occurred while trying to update customer in database",
                Details = $"{ex.Message}<br><br>{ex.StackTrace}"
            };

        }

        IsSaving = false;

    }

}
