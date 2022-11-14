using ApplicationCore.Features.Labels.Contracts;
using ApplicationCore.Features.Labels.Domain;
using ApplicationCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ApplicationCore.Features.Orders.Release.Handlers;

internal class OrderLabelHandler : DomainListener<TriggerOrderReleaseNotification> {

    private readonly ILogger<OrderLabelHandler> _logger;
    private readonly IBus _bus;
    private readonly IUIBus _uibus;

    public OrderLabelHandler(ILogger<OrderLabelHandler> logger, IBus bus, IUIBus uibus) {
        _bus = bus;
        _uibus = uibus;
        _logger = logger;
    }

    public override async Task Handle(TriggerOrderReleaseNotification notification) {

        Debug.WriteLine("** Printing Label **");

        if (!notification.ReleaseProfile.PrintOrderLabel) {
            _uibus.Publish(new OrderReleaseInfoNotification("Not printing order label, because option was disabled"));
            return;
        }

        var order = notification.Order;

        var configuration = new LabelPrinterConfiguration(notification.ReleaseProfile.OrderLabelTemplateFilePath);
        var fields = new Dictionary<string, string>() {
                { "OrderNumber", order.Number },
                { "OrderName", order.Name },
            };
        foreach (var field in order.Info) {
            fields.Add(field.Key, field.Value);
        }
        var label = new Label(fields);

        var response = await _bus.Send(new PrintLabelRequest(label, configuration));

        response.Match(
            _ => {
                _logger.LogInformation("Printed order label");
                _uibus.Publish(new OrderReleaseSuccessNotification($"Order label printed"));
            },
            error => {
                _logger.LogInformation("Error printing order label {Error}", error);
                _uibus.Publish(new OrderReleaseErrorNotification($"Error printing order label\n{error.Details}"));
            }
        );

    }
}
