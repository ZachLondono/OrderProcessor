using Domain.Orders.Entities;
using Microsoft.Extensions.Logging;
using Domain.Orders.Persistance;
using Domain.Infrastructure.Bus;
using ApplicationCore.Features.Orders.OderLoading.Dialog;
using OrderLoading;

namespace ApplicationCore.Features.Orders.OrderLoading.Dialog;

public class OrderLoadWidgetViewModel : IOrderLoadWidgetViewModel {

    private readonly ILogger<OrderLoadWidget> _logger;
    private readonly IBus _bus;
    private State _state = State.Unknown;
    private Guid _loadedOrderId = Guid.Empty;

    public List<OrderLoadMessage> Messages { get; set; } = new();

    public Guid LoadedOrderId => _loadedOrderId;

    public State State {
        get => _state;
        set {
            _state = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public Action? OnPropertyChanged { get; set; }
    public Action? OnMessageAdded { get; set; }

    public OrderLoadWidgetViewModel(ILogger<OrderLoadWidget> logger, IBus bus) {
        _logger = logger;
        _bus = bus;
    }

    public void AddLoadingMessage(MessageSeverity severity, string message) {
        Messages.Add(new() {
            Severity = severity,
            Message = message
        });
        OnMessageAdded?.Invoke();
    }

    public async Task LoadOrderFromSourceAsync(IOrderProvider orderProvider) {

        State = State.Loading;

        var data = await LoadOrderDataFromSourceAsync(orderProvider);

        if (data is null) {
            _logger.LogWarning("No order data was loaded");
            return;
        }

        var order = await CreateOrderFromDataAsync(data);

        if (order is null) {
            _logger.LogWarning("No order was created from order data {Data}", data);
            State = State.Error;
            return;
        }

        _loadedOrderId = order.Id;
        State = State.Complete;

    }

    private async Task<OrderData?> LoadOrderDataFromSourceAsync(IOrderProvider provider) {

        OrderData? data = null;

        try {

            OrderData? newData = await provider.LoadOrderData(AddLoadingMessage);

            if (newData is not null) {
                data = newData;
            } else {
                _logger.LogError("Error loading order data");
                AddLoadingMessage(MessageSeverity.Error, $"Order data could not be read from the provided order source");
                State = State.Error;
            }

        } catch (Exception ex) {

            AddLoadingMessage(MessageSeverity.Error, $"Exception thrown while trying to load order data - {ex.Message}");
            _logger.LogError(ex, "Exception thrown while trying to load order data.");
            State = State.Error;

        }

        return data;

    }

    private async Task<Order?> CreateOrderFromDataAsync(OrderData data) {

        Order? order = null;

        try {

            order = Order.Create("",
                                 data.Number.Trim(),
                                 data.Name.Trim(),
                                 string.Empty,
                                 data.WorkingDirectory.Trim(),
                                 data.CustomerId,
                                 data.VendorId,
                                 data.Comment.Trim(),
                                 data.OrderDate,
                                 data.DueDate,
                                 data.Shipping,
                                 data.Billing,
                                 data.Tax,
                                 data.PriceAdjustment,
                                 data.Rush,
                                 data.Info,
                                 data.Products,
                                 data.AdditionalItems,
                                 data.Hardware);

            var result = await _bus.Send(new InsertOrder.Command(order));
            result.OnError(error => {
                _logger.LogError("Error creating order from data {Data} {Error}", data, error);
                AddLoadingMessage(MessageSeverity.Error, error.Title + " - " + error.Details);
                order = null;
            });

        } catch (Exception ex) {

            AddLoadingMessage(MessageSeverity.Error, $"Exception thrown while trying to create new order - {ex.Message}");
            order = null;

        }

        return order;

    }

}
