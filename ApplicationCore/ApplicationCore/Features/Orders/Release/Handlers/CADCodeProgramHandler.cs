using ApplicationCore.Features.CADCode.Contracts;
using ApplicationCore.Features.CADCode.Contracts.Machining;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Orders.Release.Handlers;

internal class CADCodeProgramHandler : DomainListener<TriggerOrderReleaseNotification> {

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

    public override async Task Handle(TriggerOrderReleaseNotification notification) {

        if (!notification.ReleaseProfile.GenerateCNCPrograms) {
            _uibus.Publish(new OrderReleaseProgressNotification("Not generating CADCode CNC release because option was disabled"));
            return;
        }

        _uibus.Publish(new OrderReleaseProgressNotification("Starting CADCode CNC release"));

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
                Material = new() { Name = bottom.MaterialName, Thickness = bottom.Thickness.AsMillimeters() },
                Tokens = new List<Token>()
            };

            parts.Add(part);

        }

        var batch = new CNCBatch() {
            Name = $"{notification.Order.Number} - {notification.Order.Name}",
            Parts = parts
        };

        var response = await _bus.Send(new CNCReleaseRequest(batch, notification.ReleaseProfile.CNCReportOutputDirectory));

        response.Match(

            filePaths => {
                foreach (var file in filePaths)
                    _uibus.Publish(new OrderReleaseProgressNotification($"CNC job report created {file}"));
            },
            error => {
                _uibus.Publish(new OrderReleaseProgressNotification("Error releasing CNC programs"));
            }
        );

    }

}