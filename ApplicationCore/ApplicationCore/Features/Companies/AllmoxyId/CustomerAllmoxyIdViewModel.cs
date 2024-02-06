using Domain.Companies.AllmoxyId.Commands;
using Domain.Companies.AllmoxyId.Queries;
using ApplicationCore.Infrastructure.Bus;

namespace Domain.Companies.AllmoxyId;

public class CustomerAllmoxyIdViewModel {

    private readonly IBus _bus;

    public Action? OnPropertyChanged { get; set; }

    public Guid? CustomerId { get; set; }

    private Model? model = null;
    public Model? Model {
        get => model;
        set {
            model = value;
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

    private bool _isLoading = false;
    public bool IsLoading {
        get => _isLoading;
        set {
            _isLoading = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public CustomerAllmoxyIdViewModel(IBus bus) {
        _bus = bus;
    }

    public async Task Loaded() {

        if (CustomerId is null) return;

        var response = await _bus.Send(new GetAllmoxyId.Query((Guid)CustomerId));

        int? allmoxyId = response.Match(
                allmoxyId => allmoxyId,
                error => {
                    Error = "Could not load customer's Allmoxy Id";
                    return null;
                }
            );

        if (allmoxyId is not int id) {
            return;
        }

        Model = new() {
            CustomerId = (Guid)CustomerId,
            AllmoxyId = id,
            IsNewId = false
        };

        Error = null;

    }

    public void AddNewAllmoxyId() {

        if (CustomerId is null) {
            Error = "No Customer Set";
            return;
        }

        Model = new() {
            CustomerId = (Guid)CustomerId,
            AllmoxyId = 0,
            IsNewId = true
        };

        Error = null;

    }

    public async Task SaveChanges() {

        IsLoading = true;

        if (Model is null) return;

        if (Model.IsNewId) {

            bool wasAdded = await TryAddCustomerAllmoxyId(Model.CustomerId, Model.AllmoxyId);
            Model.IsNewId = !wasAdded;

        } else {

            await UpdateCustomerAllmoxyId(Model.CustomerId, Model.AllmoxyId);

        }

        IsLoading = false;

    }

    public async Task UpdateCustomerAllmoxyId(Guid customerId, int allmoxyId) {

        try {

            _ = await _bus.Send(new UpdateAllmoxyId.Command(customerId, allmoxyId));

        } catch (Exception ex) {

            Error = $"Failed to update allmoxy Id - {ex.Message}";

        }

    }

    public async Task<bool> TryAddCustomerAllmoxyId(Guid customerId, int allmoxyId) {

        try {

            var result = await _bus.Send(new InsertAllmoxyId.Command(customerId, allmoxyId));

            return result.Match(
                _ => true,
                _ => false);

        } catch (Exception ex) {

            Error = $"Failed to add new allmoxy Id - {ex.Message}";

            return false;

        }

    }

}
