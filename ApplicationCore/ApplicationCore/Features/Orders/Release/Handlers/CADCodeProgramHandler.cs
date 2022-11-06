using ApplicationCore.Features.CADCode.Contracts;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Orders.Release.Handlers;

internal class CADCodeProgramHandler : INotificationHandler<TriggerOrderReleaseNotification> {

    private readonly ILogger<ADuiePyleLabelHandler> _logger;
    private readonly IBus _bus;
    private readonly IUIBus _uibus;
    private readonly ConstructionValues _construction;

    public CADCodeProgramHandler(ILogger<ADuiePyleLabelHandler> logger, IBus bus, IUIBus uibus, ConstructionValues construction) {
        _bus = bus;
        _uibus = uibus;
        _logger = logger;
        _construction = construction;
    }

    public async Task Handle(TriggerOrderReleaseNotification notification, CancellationToken cancellationToken) {

        _uibus.Publish(new OrderReleaseProgressNotification("Starting CADCode CNC release"));

        //if (true) return;

        var bottoms = notification.Order.Boxes.SelectMany(b => b.GetParts(_construction).Where(p => p.Type == DrawerBoxPartType.Bottom));

        var parts = new List<CNCPart>();

        int index = 1;
        foreach (var bottom in bottoms) {

            var part = new CNCPart() {
                FileName = $"Bottom{index++}",  // TODO: encode more part informaiton in file name
                Description = "Drawer Box Bottom",
                Width = bottom.Width.AsMillimeters(),
                Length = bottom.Length.AsMillimeters(),
                Qty = bottom.Qty,
                Material = new() { Name = bottom.MaterialName, Thickness = 6.35 } // TODO: get material thickness from part
            };

            parts.Add(part);

        }

        var batch = new CNCBatch() {
            Name = $"{notification.Order.Number} - {notification.Order.Number}",
            Parts = parts
        };

        var response = await _bus.Send(new CNCReleaseRequest(batch), cancellationToken);

        response.Match(

            _ => {
                _uibus.Publish(new OrderReleaseProgressNotification("CNC programs released"));
            },
            error => {
                _uibus.Publish(new OrderReleaseProgressNotification("Error releasing CNC programs"));
            }
        );

    }

}