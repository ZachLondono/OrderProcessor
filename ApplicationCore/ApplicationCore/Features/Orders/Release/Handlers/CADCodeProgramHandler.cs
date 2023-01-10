using ApplicationCore.Features.CNC.GCode;
using ApplicationCore.Features.CNC.GCode.Contracts;
using ApplicationCore.Features.CNC.GCode.Contracts.Machining;
using ApplicationCore.Features.CNC.GCode.Contracts.Options;
using ApplicationCore.Features.Orders.Domain;
using ApplicationCore.Features.Orders.Domain.Products;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Queries;
using ApplicationCore.Infrastructure;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Release.Handlers;

internal class CADCodeProgramHandler : DomainListener<TriggerOrderReleaseNotification> {

    private readonly IBus _bus;
    private readonly IUIBus _uibus;
    private readonly ConstructionValues _construction;

    public CADCodeProgramHandler(IBus bus, IUIBus uibus, ConstructionValues construction) {
        _bus = bus;
        _uibus = uibus;
        _construction = construction;
    }

	public override async Task Handle(TriggerOrderReleaseNotification notification) {

		if (!notification.ReleaseProfile.GenerateCNCPrograms) {
			_uibus.Publish(new OrderReleaseInfoNotification("Not generating CADCode CNC release because option was disabled"));
			return;
		}

		_uibus.Publish(new OrderReleaseInfoNotification("Starting CADCode CNC release"));

        var bottoms = notification.Order
								.Products
                                .Where(p => p is IDrawerBoxContainer)
                                .Cast<IDrawerBoxContainer>()
                                .SelectMany(c => c.GetDrawerBoxes())
                                .Where(p => p is DovetailDrawerBox)
                                .Cast<DovetailDrawerBox>()
                                .SelectMany(b => b.GetParts(_construction).Where(p => p.Type == DrawerBoxPartType.Bottom))
                                .ToList();

		string batchName = $"{notification.Order.Number} - {notification.Order.Name}";

		var batchTasks = bottoms.GroupBy(b => b.MaterialId)
								.Select(g => CreateBatch(g, batchName));

		var batches = await Task.WhenAll(batchTasks);

		foreach (var batch in batches) {
			await ReleaseBatch(batch);
		}

    }

    private async Task<Batch> CreateBatch(IGrouping<Guid, DrawerBoxPart> bottoms, string batchName) {

		var matResponse = await _bus.Send(new GetDrawerBoxMaterialById.Query(bottoms.Key));
		var material = new DrawerBoxMaterial(Guid.Empty, "UNKNOWN", Dimension.FromMillimeters(0));
		matResponse.Match(
			m => {
				if (m is not null) material = m;
			},
			error => { }
		);

		int index = 1;
		var parts = bottoms.Select(bottom =>
			new Part() {
				Qty = bottom.Qty,
				Length = bottom.Width,
				Width = bottom.Length,
				Description = "Drawer Box Bottom",
				ContainsShape = false,
				PrimaryFace = new() {
					FileName = $"Bottom{index++}",  // TODO: encode more part informaiton in file name
					Operations = new List<MachiningOperation>()
				},
				SecondaryFace = null,
				LabelFields = Enumerable.Empty<LabelField>()
			}).ToList();

		return new Batch() {
			Name = batchName,
			Parts = parts,
			Material = new BatchMaterial() {
				SheetStock = material.Name,
				Thickness = material.Thickness,
			},
			LabelFields = Enumerable.Empty<LabelField>()
		};

	}

    private async Task ReleaseBatch(Batch batch) {

		var options = new GCodeGenerationOptions() {
			Machines = new List<MachineGCodeOptions>() {
				new() {
					Name = "Andi",
					GenerateLabels = true,
					GenerateNestPrograms = true,
					GenerateSinglePartPrograms = true
				}
			}
		};

		var response = await _bus.Send(new GenerateGCode.Command(batch, options));

		// TODO: generate PDF

    }

}