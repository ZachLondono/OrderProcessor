using ApplicationCore.Features.Orders.OrderLoading;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Infrastructure.Bus;
using Microsoft.Extensions.Logging;
using ApplicationCore.Features.Orders.OrderLoading.Models;
using ApplicationCore.Features.Orders.Shared.State;

namespace ApplicationCore.Features.Orders.OrderLoading.Dialog;

internal class OrderLoadWidgetViewModel : IOrderLoadWidgetViewModel {

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

    public async Task LoadOrderFromSourceAsync(OrderSourceType sourceType, string source) {

        State = State.Loading;

        var data = await LoadOrderDataFromSourceAsync(sourceType, source);

        if (data is null) {
            _logger.LogWarning("No order data was loaded from {Source} of type {SourceType}", source, sourceType);
            return;
        }

        var order = await CreateOrderFromDataAsync(source, data);

        if (order is null) {
            _logger.LogWarning("No order was created from order data {Data}", data);
            return;
        }

        _loadedOrderId = order.Id;
        State = State.Complete;

    }

    private async Task<OrderData?> LoadOrderDataFromSourceAsync(OrderSourceType sourceType, string source) {

        OrderData? data = null;

        try {

            var result = await _bus.Send(new LoadOrderCommand.Command(sourceType, source, this));

            result.Match(
                newData => {
                    data = newData;
                },
                error => {
                    _logger.LogError("Error loading order data {Source} {SourceType} {Error}", source, sourceType, error);
                    AddLoadingMessage(MessageSeverity.Error, error.Title + " - " + error.Details);
                    State = State.Error;
                }
            );

        } catch (Exception ex) {

            AddLoadingMessage(MessageSeverity.Error, $"Exception thrown while trying to load order data - {ex.Message}");
            State = State.Error;

        }

        return data;

    }

    private async Task<Order?> CreateOrderFromDataAsync(string source, OrderData data) {

        var billing = new BillingInfo() {
            InvoiceEmail = null,
            PhoneNumber = "",
            Address = new()
        };

        Order? order = null;

        try {

            order = Order.Create(source, data.Number, data.Name, string.Empty, data.WorkingDirectory, data.CustomerId, data.VendorId, data.Comment, data.OrderDate, data.Shipping, billing, data.Tax, data.PriceAdjustment, data.Rush, data.Info, data.Products, data.AdditionalItems);

            var result = await _bus.Send(new InsertOrder.Command(order));
            result.OnError(error => {
                _logger.LogError("Error creating order from data {Data} {Error}", data, error);
                AddLoadingMessage(MessageSeverity.Error, error.Title + " - " + error.Details);
                State = State.Error;
            });

        } catch (Exception ex) {

            AddLoadingMessage(MessageSeverity.Error, $"Exception thrown while trying to create new order - {ex.Message}");
            State = State.Error;

        }

        return order;

    }

}
