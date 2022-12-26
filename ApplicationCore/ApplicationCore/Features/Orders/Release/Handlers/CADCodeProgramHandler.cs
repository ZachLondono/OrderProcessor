using ApplicationCore.Features.CNC;
using ApplicationCore.Features.CNC.GCode;
using ApplicationCore.Features.CNC.GCode.Contracts;
using ApplicationCore.Features.CNC.GCode.Contracts.Machining;
using ApplicationCore.Features.CNC.GCode.Contracts.Options;
using ApplicationCore.Features.CNC.GCode.Contracts.Results;
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
		List<Batch> batches = await CreateCNCBatches(notification);

		var options = GetGCodeOptions();

		foreach (var batch in batches) { 
			
			var response = await _bus.Send(new GenerateGCode.Command(batch, options));

			await response.OnSuccessAsync(result => {
				return GeneratePDFFromGCodeResult(result, batch, notification.ReleaseProfile.CNCReportOutputDirectory);
			});

			response.OnError(error => {
				_uibus.Publish(new OrderReleaseErrorNotification($"{error.Title} - {error.Details}"));
			});

		}

	}

	private static GCodeGenerationOptions GetGCodeOptions() => new() {
		Machines = new List<MachineGCodeOptions>() {
			new() {
				GenerateLabels = true,
				GenerateNestPrograms = true,
				GenerateSinglePartPrograms = true,
				Name = "Andi"
			}
		}
	};

	private async Task GeneratePDFFromGCodeResult(GCodeGenerationResult result, Batch batch, string outputDirectory) {
		
		var job = GCodeToReleasedJobConverter.ConvertResult(result.MachineResults, result.GeneratedPicturesDirectory, batch.Name, batch.Parts);

		var pdfResponse = await _bus.Send(new GenerateCNCReleasePDF.Command(job, outputDirectory));
		pdfResponse.OnSuccess(pdfResult => {
			foreach (var file in pdfResult.FilePaths)
				_uibus.Publish(new OrderReleaseSuccessNotification($"CNC job report created {file}"));
		});

		pdfResponse.OnError(error => {
			_uibus.Publish(new OrderReleaseErrorNotification($"{error.Title} - {error.Details}"));
		});

	}

	private async Task<List<Batch>> CreateCNCBatches(TriggerOrderReleaseNotification notification) {
		
		var bottomGroups = notification.Order
								.Boxes
								.SelectMany(b => b.GetParts(_construction).Where(p => p.Type == DrawerBoxPartType.Bottom))
								.GroupBy(b => b.MaterialId);

		var batches = new List<Batch>();

		foreach (var group in bottomGroups) {

			var material = await GetMaterialById(group.Key);
			var batchMaterial = new BatchMaterial() {
				SheetStock = material.Name,
				Thickness = material.Thickness
			};

			var parts = new List<Part>();
			int index = 1;
			foreach (var bottom in group) { 
				var part = CreateCNCPart(bottom, index);
				parts.Add(part);
				index++;
			}

			var batch = new Batch() {
				Name = $"{notification.Order.Number} - {notification.Order.Name}",
				Material = batchMaterial,
				Parts = parts,
				LabelFields = new List<LabelField>()
			};

			batches.Add(batch);

		}

		return batches;
	}

	private static Part CreateCNCPart(DrawerBoxPart bottom, int index)
		=> new() {
			Description = "Drawer Box Bottom",
			Length = bottom.Width,
			Width = bottom.Length,
			ContainsShape = false,
			Qty = bottom.Qty,
			LabelFields = new List<LabelField>(),
			PrimaryFace = new() {
				FileName = $"Bottom{index}",
				Operations = Enumerable.Empty<MachiningOperation>()
			},
			SecondaryFace = null
		};

	private async Task<DrawerBoxMaterial> GetMaterialById(Guid materialId) {
		var matResponse = await _bus.Send(new GetDrawerBoxMaterialById.Query(materialId));
		
		DrawerBoxMaterial material = new(Guid.Empty, "UNKNOWN", Dimension.FromMillimeters(0));

		matResponse.OnSuccess(m => {
			if (m is not null) material = m;
		});

		matResponse.OnError(error => _logger.LogWarning("Unrecognized drawer box material id {materialId}", materialId));

		return material;
	}
}