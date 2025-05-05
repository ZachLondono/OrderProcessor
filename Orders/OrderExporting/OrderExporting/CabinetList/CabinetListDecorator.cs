using OrderExporting.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OrderExporting.CabinetList;

public class CabinetListDecorator(CabinetList data) : IDocumentDecorator {

    public CabinetList Data { get; init; } = data;

    public void Decorate(IDocumentContainer container) {

        container.Page(page => {

            page.Size(PageSizes.Letter);
            page.Margin(30);

            page.Header()
                .AlignCenter()
                .Text(t => {

                    t.Span(Data.OrderName)
                      .Bold()
                      .FontSize(16);

                    t.EmptyLine();

                    t.Span("Cabinet List")
                      .Bold()
                      .FontSize(16);

                });

            page.Content()
                .Column(col => {


                    col.Item()
                        .PaddingTop(10)
                        .Table(table => {

                            table.ColumnsDefinition(d => {

                                d.ConstantColumn(50);
                                d.ConstantColumn(50);
                                d.RelativeColumn();
                                d.ConstantColumn(50);
                                d.ConstantColumn(50);
                                d.ConstantColumn(50);
                                d.ConstantColumn(50);
                                d.ConstantColumn(50);
                                d.ConstantColumn(50);

                            });

                            table.Header(h => {

                                var addHeader = (string title) => {

                                    h.Cell()
                                     .Background("E2E8F0")
                                     .Border(1)
                                     .AlignCenter()
                                     .PaddingVertical(10)
                                     .Text(title)
                                     .Bold();

                                };

                                addHeader("#");
                                addHeader("Qty");
                                addHeader("Description");
                                addHeader("W");
                                addHeader("H");
                                addHeader("D");
                                addHeader("Hng");
                                addHeader("L");
                                addHeader("R");

                            });

                            foreach (var cab in Data.Cabinets.OrderBy(c => c.CabNum)) {

                                TextSpanDescriptor addCenteredCell(object value) =>
                                    table.Cell()
                                         .Border(1)
                                         .AlignCenter()
                                         .Text(value.ToString());

                                var hng = cab.HingedSide switch {
                                    HingedSide.Left => "L",
                                    HingedSide.Right => "R",
                                    HingedSide.Both => "B",
                                    HingedSide.NA => "N/A",
                                    _ => "?"
                                };

                                addCenteredCell(cab.CabNum);
                                addCenteredCell(cab.Qty);
                                table.Cell()
                                    .Border(1)
                                    .PaddingLeft(5)
                                    .Text(cab.Description);
                                addCenteredCell(cab.Width);
                                addCenteredCell(cab.Height);
                                addCenteredCell(cab.Depth);
                                addCenteredCell(hng);
                                addCenteredCell(cab.FinishedLeft ? "F" : "");
                                addCenteredCell(cab.FinishedRight ? "F" : "");

                                AddNoteRow(true, false, cab.IsAssembled ? "ASSEMBLED" : "NOT ASSEMBLED");
                                string fronts = cab.Fronts switch {
                                    FrontsType.None => "Fronts by Others",
                                    FrontsType.Slab => "Slab Fronts",
                                    FrontsType.MDF => "MDF Fronts",
                                    _ => ""
                                };
                                AddNoteRow(false, false, fronts);

                                for (int i = 0; i < cab.Notes.Length; i++) {

									string? note = cab.Notes[i];

                                    bool isFirst = false;
                                    bool isLast = (i == cab.Notes.Length - 1);

									AddNoteRow(isFirst, isLast, note);

								}

							}

							void AddNoteRow(bool isFirst, bool isLast, string note) {
								table.Cell()
									.ColumnSpan(9)
									.BorderLeft(1)
									.BorderRight(1)
									//.Background("E4E4E7")
									.BorderBottom(isLast ? 1 : 0)
									.PaddingTop(isFirst ? 5 : 0)
									.PaddingBottom(isLast ? 5 : 0)
									.PaddingLeft(20)
									.Text(note);
							}

						});

                });

        });

    }

}
