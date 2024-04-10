using Domain.ValueObjects;
using OrderExporting.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OrderExporting.HardwareList;

public class HardwareListDecorator(HardwareList hardwareList) : IDocumentDecorator {

	private readonly HardwareList _hardwareList = hardwareList;

	public void Decorate(IDocumentContainer container) {

		container.Page(page => {

            page.Size(PageSizes.Letter);
            page.Margin(1, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(20));

            page.Header()
				.PaddingBottom(5)
				.ShowOnce()
				.AlignCenter()
				.Text("Hardware List")
				.FontSize(24)
				.Bold();

			page.Content()
				.Column(col => {

					col.Spacing(20);

					col.Item()
						.Table(t => {

						t.ColumnsDefinition(c => {
							c.RelativeColumn();
							c.ConstantColumn(100);
						});

						t.Header(header => {
							header.Cell().Border(1).AlignCenter().Padding(3).Text("Hardware Item").Bold().FontSize(16);
							header.Cell().Border(1).AlignCenter().Padding(3).Text("Qty").Bold().FontSize(16);
						});

						AddRow("Rafix Cam Fitting", _hardwareList.RafixCamCount);
						AddRow("5mm Fixing Bolt for Cams", _hardwareList.CamBoltCount);
						AddRow("Dogbones, Dbl. Dowel for Cams", _hardwareList.DoubleSidedCamBoltCount);
						AddRow("Straight 5mm Shelf Pins", _hardwareList.StraightAdjustableShelfPinsCount);
						AddRow("Locking 5mm Shelf Pins", _hardwareList.LockingAdjustableShelfPinsCount);
						AddRow("Hanging Bracket - L", _hardwareList.HangingBracketLHCount);
						AddRow("Hanging Bracket - R", _hardwareList.HangingBracketRHCount);
						AddRow("Hinge", _hardwareList.HingeCount);
						AddRow("Hinge Plate", _hardwareList.HingePlateCount);
						AddRow("Open Hanging Rod Bracket", _hardwareList.OpenHangingRodBracketCount);
						AddRow("Closed Hanging Rod Bracket", _hardwareList.ClosedHangingRodBracketCount);

						void AddRow(string item, int qty) {
							t.Cell().Border(1).Padding(3).Text(item).FontSize(14);
							t.Cell().Border(1).AlignCenter().Padding(3).Text(qty > 0 ? qty.ToString() : "-").FontSize(14);
						}

					});

					if (_hardwareList.HangingRods.Length > 0) {

						col.Item()
							.Table(t => {

							t.ColumnsDefinition(c => {
								c.RelativeColumn();
								c.ConstantColumn(150);
							});

							t.Header(header => {

								header.Cell().ColumnSpan(2).AlignCenter().Text("Hanging Rods").Bold().FontSize(24);
								header.Cell().Border(1).AlignCenter().Text("Material").Bold().FontSize(16);
								header.Cell().Border(1).AlignCenter().Text("Length (mm)").Bold().FontSize(16);

							});

							foreach (var hangingRod in _hardwareList.HangingRods) {
								AddRow(hangingRod.Material, hangingRod.Length);
							}

							void AddRow(string item, Dimension length) {
								t.Cell().Border(1).Padding(3).Text(item).FontSize(14);
								t.Cell().Border(1).AlignCenter().Padding(3).Text(length.AsMillimeters().ToString("0")).FontSize(14);
							}

						});

					}

				});

		});

	}

}