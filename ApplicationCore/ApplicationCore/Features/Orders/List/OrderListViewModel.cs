using ApplicationCore.Features.Shared.Services;
using ApplicationCore.Infrastructure.Bus;

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
            OnPropertyChanged?.Invoke();
        }
    }

    private string _errorMessage = "";
    public string ErrorMessage {
        get => _errorMessage;
        set {
            _errorMessage = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private readonly NavigationService _navigationService;
    private readonly IBus _bus;
    private readonly CompanyInfo.GetCompanyNameById _getCompanyNameById;

    public OrderListViewModel(IBus bus, NavigationService navigationService, CompanyInfo.GetCompanyNameById getCompanyNameById) {
        _bus = bus;
        _navigationService = navigationService;
        _getCompanyNameById = getCompanyNameById;
    }

    public async Task OpenOrder(Guid orderId) {
        await _navigationService.NavigateToOrderPage(orderId);
    }

    public async Task LoadOrders() {

        var response = await _bus.Send(new GetOrderList.Query());
        response.Match(
            orders => {
                Orders = orders;
            },
            errors => {
                HasError = true;
                ErrorMessage = errors.Title;
            }
        );

        if (Orders is null) {
            return;
        }

        foreach (var order in Orders) {

            if (order is null) {
                continue;
            }

            order.VendorName = await _getCompanyNameById(order.VendorId) ?? "";

        }

    }

    public async Task OpenCompanyPage(Guid companyId) {
        await _navigationService.NavigateToCompanyPage(companyId);
    }

}
