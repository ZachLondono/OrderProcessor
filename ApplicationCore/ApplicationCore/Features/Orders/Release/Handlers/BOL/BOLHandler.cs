using ApplicationCore.Features.Companies.Domain;
using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.ExcelTemplates.Contracts;
using ApplicationCore.Features.ExcelTemplates.Domain;
using ApplicationCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Orders.Release.Handlers.BOL;

internal class BOLHandler : DomainListener<TriggerOrderReleaseNotification> {

    private readonly ILogger<BOLHandler> _logger;
    private readonly IBus _bus;
    private readonly IUIBus _uibus;

    public BOLHandler(ILogger<BOLHandler> logger, IBus bus, IUIBus uibus) {
        _bus = bus;
        _uibus = uibus;
        _logger = logger;
    }

    public override async Task Handle(TriggerOrderReleaseNotification notification) {

        if (!notification.ReleaseProfile.GenerateBOL) {
            _uibus.Publish(new OrderReleaseInfoNotification("Not creating BOL, because option was disabled"));
            return;
        }

        var order = notification.Order;

        var config = new ClosedXMLTemplateConfiguration() { TemplateFilePath = notification.ReleaseProfile.BOLTemplateFilePath };
        var outputDir = notification.ReleaseProfile.BOLOutputDirectory;
        var doPrint = notification.ReleaseProfile.PrintBOL;

        var model = new Model() {
            CustomerName = order.Customer.Name,
            CustomerStreet1 = order.Shipping.Address.Line1,
            CustomerStreet2 = order.Shipping.Address.Line2,
            CustomerCityState = $"{order.Shipping.Address.City} {order.Shipping.Address.State}",
            CustomerZip = order.Shipping.Address.Zip,
            CustomerPhone = order.Shipping.PhoneNumber
        };

        var bolResponse = await _bus.Send(new FillTemplateRequest(model, outputDir, $"{order.Number} - {order.Name} BOL", doPrint, config));

        bolResponse.Match(
            response => {
                _logger.LogInformation("BOL created: {FilePath}", response.FilePath);
                _uibus.Publish(new OrderReleaseSuccessNotification($"BOL created {response.FilePath}"));
            },
            error => {
                _logger.LogInformation("Error creating BOL {Error}", error);
                _uibus.Publish(new OrderReleaseErrorNotification($"Error creating BOL {error.Details}"));
            }
        );

    }

}
