using ApplicationCore.Features.Shared.Services;
using ApplicationCore.Infrastructure.Bus;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Orders.List;

public class OrderListViewModel {

    public Action? OnPropertyChanged { get; set; }

    private IEnumerable<OrderListItem>? _orders = null;
    public IEnumerable<OrderListItem>? Orders {
        get => _orders;
        set {
            _orders = value;
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

    private bool _hasError = false;
    public bool HasError {
        get => _hasError;
        set {
            _hasError = value;
            if (value) IsLoading = false;
            else ErrorMessage = null;
            OnPropertyChanged?.Invoke();
        }
    }

    private string? _errorMessage = null;
    public string? ErrorMessage {
        get => _errorMessage;
        set {
            _errorMessage = value;
            if (value is not null) HasError = true;
            OnPropertyChanged?.Invoke();
        }
    }


    private readonly ILogger<OrderListViewModel> _logger;
    private readonly IBus _bus;
    private readonly NavigationService _navigationService;
    private readonly CompanyInfo.GetCompanyNameById _getCompanyNameById;

    public OrderListViewModel(ILogger<OrderListViewModel> logger, IBus bus, NavigationService navigationService, CompanyInfo.GetCompanyNameById getCompanyNameById) {
        _logger = logger;
        _bus = bus;
        _navigationService = navigationService;
        _getCompanyNameById = getCompanyNameById;
    }

    public async Task OpenOrder(Guid orderId) {
        await _navigationService.NavigateToOrderPage(orderId);
    }

    public async Task LoadOrders() {

        IsLoading = true;
        HasError = false;

        var response = await _bus.Send(new GetOrderList.Query());
        response.Match(
            orders => {
                Orders = orders;
            },
            errors => {
                ErrorMessage = errors.Title;
            }
        );

        if (Orders is null) {
            IsLoading = false;
            return;
        }

        foreach (var order in Orders) {

            if (order is null) {
                continue;
            }

            order.VendorName = await _getCompanyNameById(order.VendorId) ?? "";

        }

        IsLoading = false;

    }

    public async Task OpenCompanyPage(Guid companyId) {
        await _navigationService.NavigateToCompanyPage(companyId);
    }

}
