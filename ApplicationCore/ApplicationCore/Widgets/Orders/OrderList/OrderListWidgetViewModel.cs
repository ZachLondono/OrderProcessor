using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Shared.Services;
using ApplicationCore.Infrastructure.Bus;
using Microsoft.Extensions.Logging;
using ApplicationCore.Features.Orders.List;

namespace ApplicationCore.Widgets.Orders.OrderList;

public class OrderListWidgetViewModel {

    public Action? OnPropertyChanged { get; set; }

    private IEnumerable<OrderListItem>? _orders = null;
    private bool _isLoading = false;
    private bool _hasError = false;
    private string? _errorMessage = null;
    private int _pageCount = 0;
    private int _pageSize = 10;
    private int _totalOrderCount = 0;

    public IEnumerable<OrderListItem>? Orders {
        get => _orders;
        set {
            _orders = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public bool IsLoading {
        get => _isLoading;
        set {
            _isLoading = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public bool HasError {
        get => _hasError;
        set {
            _hasError = value;
            if (value) IsLoading = false;
            else ErrorMessage = null;
            OnPropertyChanged?.Invoke();
        }
    }

    public string? ErrorMessage {
        get => _errorMessage;
        set {
            _errorMessage = value;
            if (value is not null) HasError = true;
            OnPropertyChanged?.Invoke();
        }
    }

    public int PageCount {
        get => _pageCount;
        set {
            _pageCount = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public int PageSize {
        get => _pageSize;
        set {
            _pageSize = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public int TotalOrderCount {
        get => _totalOrderCount;
        set {
            _totalOrderCount = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public int Page { get; set; } = 1;

    private readonly ILogger<OrderListWidgetViewModel> _logger;
    private readonly IBus _bus;
    private readonly NavigationService _navigationService;
    private readonly CompanyDirectory.GetVendorNameByIdAsync _getVendorNameById;
    private readonly CompanyDirectory.GetCustomerNameByIdAsync _getCustomerNameById;

    public OrderListWidgetViewModel(ILogger<OrderListWidgetViewModel> logger, IBus bus, NavigationService navigationService, CompanyDirectory.GetVendorNameByIdAsync getVendorNameById, CompanyDirectory.GetCustomerNameByIdAsync getCustomerNameById) {
        _logger = logger;
        _bus = bus;
        _navigationService = navigationService;
        _getVendorNameById = getVendorNameById;
        _getCustomerNameById = getCustomerNameById;
    }

    public void OpenOrder(Guid orderId) {
        _navigationService.NavigateToOrderPage(orderId);
    }

    public async Task LoadOrders(Guid? customerId, Guid? vendorId, string? searchTerm) {

        IsLoading = true;
        HasError = false;

        var countResponse = await _bus.Send(new GetOrderCount.Query(customerId, vendorId, searchTerm));
        countResponse.OnSuccess(
            totalOrders => {
                TotalOrderCount = totalOrders;
                PageCount = (int)Math.Ceiling((float)totalOrders / (float)PageSize);
            });

        var response = await _bus.Send(new GetOrderList.Query(customerId, vendorId, searchTerm, Page, PageSize));
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

        Dictionary<Guid, string> customerNames = new();
        Dictionary<Guid, string> vendorNames = new();
        foreach (var order in Orders) {

            if (order is null) {
                continue;
            }

            if (vendorNames.TryGetValue(order.VendorId, out string? vendorName)) {
                order.VendorName = vendorName ?? "";
            } else {
                order.VendorName = await _getVendorNameById(order.VendorId) ?? "";
                vendorNames[order.VendorId] = order.VendorName;
            }

            if (customerNames.TryGetValue(order.CustomerId, out string? customerName)) {
                order.CustomerName = customerName ?? "";
            } else {
                order.CustomerName = await _getCustomerNameById(order.CustomerId) ?? "";
                customerNames[order.CustomerId] = order.CustomerName;
            }

        }

        IsLoading = false;

    }

    public void OpenCustomerPage(Guid companyId) {
        _navigationService.NavigateToCustomerPage(companyId);
    }

    public void OpenVendorPage(Guid companyId) {
        _navigationService.NavigateToVendorPage(companyId);
    }

}
