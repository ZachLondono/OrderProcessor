using ApplicationCore.Features.Labels.Contracts;
using ApplicationCore.Features.Labels.Domain;
using ApplicationCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Orders.Release.Handlers;

internal class ADuiePyleLabelHandler : DomainListener<TriggerOrderReleaseNotification> {

    private readonly ILogger<ADuiePyleLabelHandler> _logger;
    private readonly IBus _bus;
    private readonly IUIBus _uibus;
    
    public ADuiePyleLabelHandler(ILogger<ADuiePyleLabelHandler> logger, IBus bus, IUIBus uibus) {
        _bus = bus;
        _uibus = uibus;
        _logger = logger;
    }

    public override async Task Handle(TriggerOrderReleaseNotification notification) {

        if (!notification.ReleaseProfile.PrintADuiePyleLabel) {
            _uibus.Publish(new OrderReleaseProgressNotification("Not printing a duie pyle label, because option was disabled"));
            return;
        }
        var order = notification.Order;

        var configuration = new LabelPrinterConfiguration(notification.ReleaseProfile.ADuiePyleLabelTemplateFilePath);
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
                _logger.LogInformation("Printed A Duie Pyle label");
                _uibus.Publish(new OrderReleaseProgressNotification($"A duie pyle label printed"));
            },
            error => {
                _logger.LogInformation("Error printing A Duie Pyle label {Error}", error);
                _uibus.Publish(new OrderReleaseProgressNotification($"Error printing A Duie Pyle labels\n{error.Message}"));
            }
        );

    }
}
