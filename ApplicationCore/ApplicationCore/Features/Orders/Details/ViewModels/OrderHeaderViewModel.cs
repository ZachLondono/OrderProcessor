using ApplicationCore.Features.Companies.Customers.Queries;
using ApplicationCore.Features.Companies.Vendors.Queries;
using ApplicationCore.Features.Orders.Details.Models;
using ApplicationCore.Features.Orders.Shared.State;
using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Services;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Orders.Details.ViewModels;

public class OrderHeaderViewModel {

    private readonly IBus _bus;
    private readonly ILogger<OrderHeaderViewModel> _logger;
    private readonly NavigationService _navService;

    public Action<Error>? OnErrorOccurred { get; set; }
    public Action? OnPropertyChanged { get; set; }

    private OrderHeaderModel _model = new();
    public OrderHeaderModel Model {
        get => _model;
        private set {
            _model = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private bool _isLoading = true;
    public bool IsLoading {
        get => _isLoading;
        private set {
            _isLoading = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public OrderHeaderViewModel(IBus bus, ILogger<OrderHeaderViewModel> logger, NavigationService navService) {
        _bus = bus;
        _logger = logger;
        _navService = navService;
    }

    public async Task LoadOrderHeaderAsync(Guid orderId) {

        try {

            var response = await _bus.Send(new GetOrderHeader.Query(orderId));
            response.OnSuccess(async header => {

                Model.OrderId = header.OrderId;
                Model.Name = header.Name;
                Model.Number = header.Number;
                Model.CustomerId = header.CustomerId;
                Model.VendorId = header.VendorId;
                Model.OrderDate = header.OrderDate;
                Model.DueDate = header.DueDate;
                Model.Rush = header.Rush;
                Model.CustomerComment = header.CustomerComment;

                var customerResponse = await _bus.Send(new GetCustomerNameById.Query(Model.CustomerId));
                customerResponse.OnSuccess(name => Model.CustomerName = name);

                var vendorResponse = await _bus.Send(new GetVendorNameById.Query(Model.VendorId));
                vendorResponse.OnSuccess(name => Model.VendorName = name);

                IsLoading = false;

            });

        } catch (Exception ex) {

            _logger.LogError(ex, "An exception occurred while trying to load order header");

        }

    }

    public async Task SetDueDateAsync(DateTime date) {
        Model.DueDate = date;
        await UpdateDueDateAsync();
        OnPropertyChanged?.Invoke();
    }

    public async Task RemoveDueDateAsync() {
        Model.DueDate = null;
        await UpdateDueDateAsync();
        OnPropertyChanged?.Invoke();
    }

    public async Task AddDueDateAsync() {
        Model.DueDate = DateTime.Today;
        await UpdateDueDateAsync();
        OnPropertyChanged?.Invoke();
    }

    private async Task UpdateDueDateAsync() {

        try {

            var result = await _bus.Send(new UpdateOrderDueDate.Command(Model.OrderId, Model.DueDate));

            result.OnError(error => OnErrorOccurred?.Invoke(error));

        } catch (Exception ex) {

            _logger.LogError(ex, "Exception thrown while trying to update order due date");

            OnErrorOccurred?.Invoke(new() {
                Title = "Failed to Update Due Date",
                Details = ex.Message
            });

        }

    }

    public void OpenCustomerPageAsync() => _navService.NavigateToCustomerPage(Model.CustomerId);

    public void OpenVendorPage() => _navService.NavigateToVendorPage(Model.VendorId);

}
