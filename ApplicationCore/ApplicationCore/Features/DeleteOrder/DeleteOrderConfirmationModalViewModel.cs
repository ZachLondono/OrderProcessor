using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Features.DeleteOrder;

public class DeleteOrderConfirmationModalViewModel {

    private string _orderNumber = string.Empty;
    private bool _isDeleting = false;
    private Error? _error = null;
    private Guid _orderId = Guid.Empty;

    public Action? OnPropertyChanged { get; set; }

    public string OrderNumber {
        get => _orderNumber;
        set {
            _orderNumber = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public bool IsDeleting {
        get => _isDeleting;
        set {
            _isDeleting = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public Error? Error {
        get => _error;
        set {
            _error = value;
            OnPropertyChanged?.Invoke();
        }
    }

    private readonly IBus _bus;

    public DeleteOrderConfirmationModalViewModel(IBus bus) {
        _bus = bus;
    }

    public async Task InitializeAsync(Guid orderId) {

        _orderId = orderId;
        var result = await _bus.Send(new GetOrderNumberById.Query(orderId));

        result.Match(
            number => OrderNumber = number,
            error => Error = error
        );

    }

    public async Task<bool> ConfirmDelete() {

        if (_orderId == Guid.Empty) {

            Error = new() {
                Title = "Could Not Delete Order",
                Details = "The order to delete was not set"
            };

            return false;

        }

        IsDeleting = true;

        bool wasDeleted = false;

        try {

            var result = await _bus.Send(new DeleteOrder.Command(_orderId));

            result.Match(
                _ => wasDeleted = true,
                error => {
                    wasDeleted = false;
                    Error = error;
                });

        } catch (Exception ex) {

            Error = new() {
                Title = "Failed to Delete Order",
                Details = $"Exception occurred thrown trying to delete order. {ex.Message}"
            };

        }

        IsDeleting = false;

        return wasDeleted;

    }

}
