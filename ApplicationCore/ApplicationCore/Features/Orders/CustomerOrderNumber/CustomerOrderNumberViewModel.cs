using Dapper;
using Domain.Infrastructure.Bus;
using Domain.Orders.Persistance;

namespace ApplicationCore.Features.Orders.CustomerOrderNumber;

internal class CustomerOrderNumberViewModel {

    private CustomerOrderNumberModel? _model = null;
    private Error? _error = null;

    public CustomerOrderNumberModel? Model {
        get => _model;
        private set {
            _model = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public Error? Error {
        get => _error;
        private set {
            _error = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public Action? OnPropertyChanged { get; set; }

    private readonly IOrderingDbConnectionFactory _factory;

    public CustomerOrderNumberViewModel(IOrderingDbConnectionFactory factory) {
        _factory = factory;
    }

    public async Task LoadData(Guid customerId) {

        try {

            using var connection = await _factory.CreateConnection();

            var model = connection.QuerySingleOrDefault<CustomerOrderNumberModel>(
                "SELECT customer_id AS CustomerId, number AS Number FROM order_numbers WHERE customer_id = @CustomerId;",
                new {
                    CustomerId = customerId
                });

            if (model is not null) {
                Model = model;
            }

            Error = null;

        } catch (Exception ex) {

            Error = new() {
                Title = "Could not load order number data",
                Details = ex.Message
            };

        }

    }

    public async Task SaveData() {

        if (Model is null) {
            Error = new() {
                Title = "Could not save data",
                Details = "There is no data available to save, try reloading data"
            };
            return;
        }

        try {

            using var connection = await _factory.CreateConnection();

            connection.Execute(
                "UPDATE order_numbers SET number = @Number WHERE customer_id = @CustomerId;",
                Model);

            Error = null;

        } catch (Exception ex) {

            Error = new() {
                Title = "Could not save order number data",
                Details = ex.Message
            };

        }

    }

    public async Task AddNewOrderNumber(Guid customerId) {

        try {

            using var connection = await _factory.CreateConnection();

            CustomerOrderNumberModel model = new() {
                CustomerId = customerId,
                Number = 1
            };

            connection.Execute(
                "INSERT INTO order_numbers  (customer_id, number) VALUES (@CustomerId, @Number);",
                model);

            Error = null;
            Model = model;

        } catch (Exception ex) {

            Error = new() {
                Title = "Could not save order number data",
                Details = ex.Message
            };

        }

    }

}
