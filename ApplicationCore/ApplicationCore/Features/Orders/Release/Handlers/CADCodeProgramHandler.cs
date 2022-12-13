using ApplicationCore.Features.CNC;
using ApplicationCore.Features.CNC.GCode;
using ApplicationCore.Features.CNC.GCode.Contracts;
using ApplicationCore.Features.CNC.GCode.Contracts.Machining;
using ApplicationCore.Features.CNC.ReleasePDF;
using ApplicationCore.Features.Orders.Domain;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Queries;
using ApplicationCore.Infrastructure;
using ApplicationCore.Shared.Domain;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Orders.Release.Handlers;

internal class CADCodeProgramHandler : DomainListener<TriggerOrderReleaseNotification> {

    private readonly ILogger<CADCodeProgramHandler> _logger;
    private readonly IBus _bus;
    private readonly IUIBus _uibus;
    private readonly ConstructionValues _construction;

    public CADCodeProgramHandler(ILogger<CADCodeProgramHandler> logger, IBus bus, IUIBus uibus, ConstructionValues construction) {
        _bus = bus;
        _uibus = uibus;
        _logger = logger;
        _construction = construction;
    }

    public override async Task Handle(TriggerOrderReleaseNotification notification) {

        if (!notification.ReleaseProfile.GenerateCNCPrograms) {
            _uibus.Publish(new OrderReleaseInfoNotification("Not generating CADCode CNC release because option was disabled"));
            return;
        }

        _uibus.Publish(new OrderReleaseInfoNotification("Starting CADCode CNC release"));

        var bottoms = notification.Order.Boxes.SelectMany(b => b.GetParts(_construction).Where(p => p.Type == DrawerBoxPartType.Bottom));

        var parts = new List<Part>();

        int index = 1;
        foreach (var bottom in bottoms) {

            var matResponse = await _bus.Send(new GetDrawerBoxMaterialById.Query(bottom.MaterialId));
            DrawerBoxMaterial material = new DrawerBoxMaterial(Guid.Empty, "UNKNOWN", Dimension.FromMillimeters(0));
            matResponse.Match(
                m => {
                    if (m is not null) material = m;
                },
                error => { }
            );

            var part = new Part() {
                FileName = $"Bottom{index++}",  // TODO: encode more part informaiton in file name
                Description = "Drawer Box Bottom",
                Length = bottom.Width.AsMillimeters(),
                Width = bottom.Length.AsMillimeters(),
                ContainsShape = false,
                Qty = bottom.Qty,
                Material = new() { Name = material.Name, Thickness = material.Thickness.AsMillimeters() },
                Tokens = new List<MachiningOperation>()
            };

            parts.Add(part);

        }

        var batch = new Batch() {
            Name = $"{notification.Order.Number} - {notification.Order.Name}",
            Parts = parts
        };

		// TODO send emails to shop manager
		var response = await _bus.Send(new GenerateGCode.Command(batch));

        await response.MatchAsync(

            async result => {

                var job = new GCodeToReleasedJobConverter().ConvertResult(result, batch.Name, batch.Parts);

                var pdfResponse = await _bus.Send(new GenerateCNCReleasePDF.Command(job, notification.ReleaseProfile.CNCReportOutputDirectory));
                pdfResponse.Match(
                    pdfResult => {
						foreach (var file in pdfResult.FilePaths)
							_uibus.Publish(new OrderReleaseSuccessNotification($"CNC job report created {file}"));
					},
                    error => {
						_uibus.Publish(new OrderReleaseErrorNotification($"{error.Title} - {error.Details}"));
					}
                );

            },
            error => {
                _uibus.Publish(new OrderReleaseErrorNotification($"{error.Title} - {error.Details}"));
                return Task.CompletedTask;
            }
        );

    }

}