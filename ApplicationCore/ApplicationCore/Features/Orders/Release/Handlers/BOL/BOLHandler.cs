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
            _uibus.Publish(new OrderReleaseProgressNotification("Not creating BOL, because option was disabled"));
            return;
        }

        var order = notification.Order;
        
        bool didError = false;
        Company? customer = null;
        var custQuery = await GetCompany(order.CustomerId);
        custQuery.Match(
            (cust) => {
                customer = cust;
            },
            (error) => {
                didError = true;
                _logger.LogError("Error loading customer {Error}", error);
            }
        );

        if (didError || customer is null) {
            _uibus.Publish(new OrderReleaseProgressNotification($"Error creating BOL, could not find customer details"));
            return;
        }

        var config = new ClosedXMLTemplateConfiguration() { TemplateFilePath = notification.ReleaseProfile.BOLTemplateFilePath };
        var outputDir = notification.ReleaseProfile.BOLOutputDirectory;
        var doPrint = notification.ReleaseProfile.PrintBOL;

        var model = new Model() {
            CustomerName = customer.Name,
            CustomerStreet1 = customer.Address.Line1,
            CustomerStreet2 = customer.Address.Line2,
            CustomerCityState = $"{customer.Address.City} {customer.Address.State}",
            CustomerZip = customer.Address.Zip,
            CustomerPhone = customer.PhoneNumber
        };

        var bolResponse = await _bus.Send(new FillTemplateRequest(model, outputDir, $"{order.Number} - {order.Name} BOL", doPrint, config));

        bolResponse.Match(
            response => {
                _logger.LogInformation("BOL created: {FilePath}", response.FilePath);
                _uibus.Publish(new OrderReleaseProgressNotification($"BOL created {response.FilePath}"));
            },
            error => {
                _logger.LogInformation("Error creating BOL {Error}", error);
                _uibus.Publish(new OrderReleaseProgressNotification($"Error creating BOL {error.Details}"));
            }
        );

    }

    private async Task<Response<Company>> GetCompany(Guid companyId) {
        return await _bus.Send(new GetCompanyById.Query(companyId));
    }

}
