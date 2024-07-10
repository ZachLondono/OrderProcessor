using Domain.ValueObjects;
using OrderExporting.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OrderExporting.HardwareList;

public class HardwareListDecorator(Hardware hardwareList) : IDocumentDecorator {

	private readonly Hardware _hardwareList = hardwareList;

	public void Decorate(IDocumentContainer container) {

		container.Page(page => {

            page.Size(PageSizes.Letter);
            page.Margin(1, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(20));

            page.Header()
				.PaddingBottom(5)
				.ShowOnce()
				.AlignCenter()
				.Text($"{_hardwareList.OrderNumber} - Hardware List")
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

                            foreach (var item in _hardwareList.Supplies.Where(i => i.Qty > 0)) {
                                AddRow(item.Description, item.Qty);
                            }

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
									c.ConstantColumn(150);
								});

								t.Header(header => {

									header.Cell().ColumnSpan(2).AlignCenter().Text("Closet Hanging Rods").Bold().FontSize(24);
									header.Cell().Border(1).AlignCenter().Text("Material").Bold().FontSize(16);
									header.Cell().Border(1).AlignCenter().Text("Length (mm)").Bold().FontSize(16);
									header.Cell().Border(1).AlignCenter().Text("Qty").Bold().FontSize(16);

								});

								foreach (var hangingRod in _hardwareList.HangingRods) {
									AddRow(hangingRod.Finish, hangingRod.Length, hangingRod.Qty);
								}

								void AddRow(string item, Dimension length, int qty) {
									t.Cell().Border(1).Padding(3).Text(item).FontSize(14);
									t.Cell().Border(1).AlignCenter().Padding(3).Text(length.AsMillimeters().ToString("0")).FontSize(14);
									t.Cell().Border(1).AlignCenter().Padding(3).Text(qty.ToString()).FontSize(14);
								}

							});

					}

					if (_hardwareList.DrawerSlides.Length > 0) {

						col.Item()
							.Table(t => {

							t.ColumnsDefinition(c => {
								c.ConstantColumn(150);
								c.ConstantColumn(150);
								c.RelativeColumn();
							});

							t.Header(header => {

								header.Cell().ColumnSpan(3).AlignCenter().Text("Drawer Slides").Bold().FontSize(24);
								header.Cell().Border(1).AlignCenter().Text("Qty").Bold().FontSize(16);
								header.Cell().Border(1).AlignCenter().Text("Length (mm)").Bold().FontSize(16);
								header.Cell().Border(1).AlignCenter().Text("Style").Bold().FontSize(16);

							});

							foreach (var hangingRod in _hardwareList.DrawerSlides) {
								AddRow(hangingRod.Qty, hangingRod.Length, hangingRod.Style);
							}

							void AddRow(int qty, Dimension length, string style) {
								t.Cell().Border(1).AlignCenter().Padding(3).Text(qty.ToString()).FontSize(14);
								t.Cell().Border(1).AlignCenter().Padding(3).Text(length.AsMillimeters().ToString("0")).FontSize(14);
								t.Cell().Border(1).Padding(3).Text(style).FontSize(14);
							}

						});

					}

				});

		});

	}

}