using ApplicationCore.Features.Labels.Contracts;
using ApplicationCore.Features.Labels.Domain;
using ApplicationCore.Features.Orders.Domain;
using ApplicationCore.Features.Orders.Domain.Products;
using ApplicationCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Orders.Release.Handlers;

internal class BoxLabelsHandler : DomainListener<TriggerOrderReleaseNotification> {

    private readonly ILogger<BoxLabelsHandler> _logger;
    private readonly IBus _bus;
    private readonly IUIBus _uibus;

    public BoxLabelsHandler(ILogger<BoxLabelsHandler> logger, IBus bus, IUIBus uibus) {
        _bus = bus;
        _uibus = uibus;
        _logger = logger;
    }

    public override async Task Handle(TriggerOrderReleaseNotification notification) {

        if (!notification.ReleaseProfile.PrintBoxLabels) {
            _uibus.Publish(new OrderReleaseInfoNotification("Not printing box labels, because option was disabled"));
            return;
        }
        var order = notification.Order;

        var configuration = new LabelPrinterConfiguration(notification.ReleaseProfile.BoxLabelsTemplateFilePath);

        var dovetailBoxes = order.Products
                                .Where(p => p is IDrawerBoxContainer)
                                .Cast<IDrawerBoxContainer>()
                                .SelectMany(c => c.GetDrawerBoxes())
                                .Where(p => p is DovetailDrawerBox)
                                .Cast<DovetailDrawerBox>()
                                .ToList();

        var labels = new List<Label>();
        foreach (var box in dovetailBoxes) {

            string sizeStr = $"{box.Height.AsInchFraction}\"Hx{box.Width.AsInchFraction}\"Wx{box.Depth.AsInchFraction}\"D";

            var fields = new Dictionary<string, string>() {
                { "OrderNumber", order.Number },
                { "OrderName", order.Name },
                { "Comment", order.CustomerComment },
                { "Qty", box.Qty.ToString() },
                { "LineNum", box.LineInOrder.ToString() },
                { "Size", sizeStr },
            };

            foreach (var field in order.Info) {
                fields.Add(field.Key, field.Value);
            }

            labels.Add(new Label(fields));

        }

        var response = await _bus.Send(new PrintLabelsRequest(labels, configuration));

        response.Match(
            _ => {
                _logger.LogInformation("Printed {Count} box labels", labels.Count);
                _uibus.Publish(new OrderReleaseSuccessNotification($"{labels.Count} box labels printed"));
            },
            error => {
                _logger.LogInformation("Error printing box labels {Error}", error);
                _uibus.Publish(new OrderReleaseErrorNotification($"Error printing box labels\n{error.Details}"));
            }
        );

    }
}
