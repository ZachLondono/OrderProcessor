using Domain.Orders.Entities;
using Domain.Orders.ValueObjects;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using Domain.Orders.Enums;
using Domain.ValueObjects;
using Domain.Extensions;
using OrderExporting.Shared;

namespace OrderExporting.JobSummary;

public class JobSummaryDecorator(JobSummary jobSummary) : IDocumentDecorator {

    private readonly JobSummary? _jobSummary = jobSummary;

    public void Decorate(IDocumentContainer container) {

        if (_jobSummary is null) {
            return;
        }

        ComposeJobSummary(container, _jobSummary);

    }

    private static void ComposeJobSummary(IDocumentContainer container, JobSummary jobSummary) {

        container.Page(page => {

            page.Size(PageSizes.Letter);
            page.Margin(1, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(20));

            page.Header().ShowOnce().Element(e => ComposeHeader(e, jobSummary));

            page.Content()
                .Section("Job Summary")
                .Column(column => {

                    column.Item()
                        .PaddingTop(30)
                        .Row(row => {

                            row.RelativeItem(1)
                                .AlignCenter()
                                .DefaultTextStyle(style => style.FontSize(12))
                                .Table(table => {

                                    table.ColumnsDefinition(column => {
                                        column.ConstantColumn(90);
                                        column.ConstantColumn(50);
                                    });

                                    List<(string name, int qty)> prodQtys = new() {
                                        ("Cabinets", jobSummary.Cabinets.Sum(p => p.Items.Sum(i => i.Qty)) ),
                                        ("Cabinet Extras", jobSummary.CabinetParts.Sum(p => p.Items.Sum(i => i.Qty)) ),
                                        ("Closet Parts", jobSummary.ClosetParts.Sum(p => p.Items.Sum(i => i.Qty)) ),
                                        ("Zargen Drawers", jobSummary.ZargenDrawers.Sum(p => p.Items.Sum(i => i.Qty)) ),
                                        ("MDF Doors", jobSummary.MDFDoors.Sum(p => p.Items.Sum(i => i.Qty)) ),
                                        ("5-Piece Doors", jobSummary.FivePieceDoors.Sum(p => p.Items.Sum(i => i.Qty)) ),
                                        ("Dovetail DBs", jobSummary.DovetailDrawerBoxes.Sum(p => p.Items.Sum(i => i.Qty)) ),
                                        ("Doweled DBs", jobSummary.DoweledDrawerBoxes.Sum(p => p.Items.Sum(i => i.Qty)) ),
                                        ("Counter Tops", jobSummary.CounterTops.Sum(p => p.Qty) ),
                                        ("Other", jobSummary.AdditionalItems.Sum(i => i.Qty) )
                                    };

                                    foreach (var (name, qty) in prodQtys) {
                                        if (qty <= 0) {
                                            continue;
                                        }
                                        table.Cell().AlignRight().PaddingRight(5).Text($"{name}:");
                                        table.Cell().AlignCenter().Text(qty == 0 ? "-" : qty.ToString());
                                    }

                                    var totalQty = prodQtys.Sum(prod => prod.qty);

                                    table.Cell().BorderTop(0.5f).AlignRight().PaddingRight(5).Text("Total:").Bold().FontSize(14);
                                    table.Cell().BorderTop(0.5f).AlignCenter().Text(totalQty.ToString()).Bold().FontSize(14);

                                });

                            if (jobSummary.ShowMaterialTypesInSummary && (jobSummary.MaterialTypes.Count != 0 || jobSummary.EdgeBandingTypes.Count != 0)) {

                                row.RelativeItem(1)
                                    .AlignLeft()
                                    .AlignTop()
                                    .DefaultTextStyle(style => style.FontSize(12))
                                    .Column(col => {

                                        if (jobSummary.MaterialTypes.Count != 0) {

                                            col.Item()
                                                .AlignLeft()
                                                .Text("Materials:")
                                                .FontSize(14)
                                                .Bold();

                                            jobSummary.MaterialTypes
                                                        .Select((mat, idx) => (mat, idx))
                                                        .ForEach(item =>
                                                            col.Item()
                                                                 .AlignLeft()
                                                                 .Text($"    {item.idx + 1})  {item.mat}")
                                                                 .FontSize(12)
                                                                 .Bold());

                                        }

                                        if (jobSummary.EdgeBandingTypes.Count != 0) {

                                            col.Item()
                                                .AlignLeft()
                                                .Text("Edge Banding:")
                                                .FontSize(14)
                                                .Bold();

                                            jobSummary.EdgeBandingTypes
                                                        .Select((eb, idx) => (eb, idx))
                                                        .ForEach(item =>
                                                            col.Item()
                                                                 .AlignLeft()
                                                                 .Text($"    {item.idx + 1})  {item.eb}")
                                                                 .FontSize(12)
                                                                 .Bold());

                                        }

                                    });
                            }

                        });

                    if (jobSummary.ContainsMDFDoorSubComponents
                        || jobSummary.ContainsDovetailDBSubComponents
                        || jobSummary.ContainsFivePieceDoorSubComponents
                        || jobSummary.InstallCamsInClosetParts) {
                        column.Item().PaddingTop(30);
                    }

                    if (jobSummary.InstallCamsInClosetParts) {
                        column.Item()
                              .AlignCenter()
                              .Text(t => {
                                    t.DefaultTextStyle(ts => ts.Bold().FontSize(16));
                                    t.Span("INSTALL CAMS").Underline();
                              });
                    }

                    if (jobSummary.ContainsDovetailDBSubComponents) {
                        column.Item()
                                .AlignCenter()
                                .Text(t => {
                                    t.DefaultTextStyle(ts => ts.Bold().FontSize(16));
                                    t.Span("CONTAINS ");
                                    t.Span("DOVETAIL DRAWER BOXES").Underline();
                                });
                    }

                    if (jobSummary.ContainsMDFDoorSubComponents) {
                        column.Item()
                                .AlignCenter()
                                .Text(t => {
                                    t.DefaultTextStyle(ts => ts.Bold().FontSize(16));
                                    t.Span("CONTAINS ");
                                    t.Span("MDF DOORS").Underline();
                                });
                    }

                    if (jobSummary.ContainsFivePieceDoorSubComponents) {
                        column.Item()
                                .AlignCenter()
                                .Text(t => {
                                    t.DefaultTextStyle(ts => ts.Bold().FontSize(16));
                                    t.Span("CONTAINS ");
                                    t.Span("5-PIECE DOORS").Underline();
                                });
                    }

                    if (!string.IsNullOrWhiteSpace(jobSummary.DoweledDBNotchMessage)) {
                        column.Item()
                                .AlignCenter()
                                .Text(jobSummary.DoweledDBNotchMessage)
                                .Bold()
                                .FontSize(16);
                    }

                    column.Item()
                            .PaddingLeft(10)
                            .PaddingTop(20)
                            .Text("Special Requirements")
                            .Italic()
                            .Bold()
                            .FontSize(16);

                    column.Item()
                            .Border(1)
                            .BorderColor(Colors.Grey.Medium)
                            .MinHeight(45)
                            .Container()
                            .Padding(5)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text(jobSummary.SpecialRequirements)
                            .FontSize(10);

                    column.Item().PaddingTop(20).PaddingBottom(20).Row(row => row.RelativeItem().LineHorizontal(1).LineColor(Colors.Grey.Medium));

                    column.Item().Row(row => {

                        row.Spacing(10);

                        if (jobSummary.AdditionalItems.Any() && jobSummary.ShowAdditionalItemsInSummary) {
                            ComposeAdditionalItemsTable(row.RelativeItem(), jobSummary.AdditionalItems);
                        }

                        if (jobSummary.CounterTops.Any() && jobSummary.ShowCounterTopsInSummary) {
                            ComposeCounterTopTable(row.RelativeItem(), jobSummary.CounterTops);
                        }

                    });

                    if (jobSummary.ShowItemsInSummary) {

                        column.Item().PageBreak();

                        foreach (var group in jobSummary.Cabinets) {
                            ComposeCabinetTable(column.Item(), group);
                        }

                        foreach (var group in jobSummary.CabinetParts) {
                            ComposeCabinetPartTable(column.Item(), group);
                        }

                        foreach (var group in jobSummary.ClosetParts) {
                            ComposeClosetPartTable(column.Item(), group);
                        }

                        foreach (var group in jobSummary.ZargenDrawers) {
                            ComposeZargenDrawerTable(column.Item(), group);
                        }

                        foreach (var group in jobSummary.DovetailDrawerBoxes) {
                            ComposeDovetailDrawerBoxTable(column.Item(), group);
                        }

                        foreach (var group in jobSummary.DoweledDrawerBoxes) {
                            ComposeDoweledDrawerBoxTable(column.Item(), group);
                        }

                        foreach (var group in jobSummary.MDFDoors) {
                            ComposeMDFDoorTable(column.Item(), group);
                        }

                        foreach (var group in jobSummary.FivePieceDoors) {
                            ComposeFivePieceDoorTable(column.Item(), group);
                        }
                    }

                });

            page.Footer()
                .AlignCenter()
                .Text(x => {

                    x.DefaultTextStyle(x => x.FontSize(12));

                    x.Span("Page ");
                    x.PageNumberWithinSection("Job Summary");
                    x.Span(" of ");
                    x.TotalPagesWithinSection("Job Summary");

                });

        });
    }
    
    private static void ComposeCabinetTable(IContainer container, CabinetGroup group) {

        var applyDefaultCellStyle = (IContainer cell, bool addBottomBorder, bool addTopBorder)
            => cell.BorderLeft(1)
                    .BorderRight(1)
                    .BorderTop(addTopBorder ? 1 : 0)
                    .BorderBottom(addBottomBorder ? 1 : 0)
                    .BorderColor(Colors.Grey.Lighten1)
                    .AlignMiddle()
                    .PaddingVertical(3)
                    .PaddingHorizontal(3);

        var headerCellStyle = (IContainer cell)
            => cell.Border(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .Background(Colors.Grey.Lighten3)
                    .AlignCenter()
                    .PaddingVertical(3)
                    .PaddingHorizontal(3)
                    .DefaultTextStyle(x => x.Bold());

        container.Column(col => {

            col.Item()
                .PaddingTop(10)
                .PaddingLeft(10)
                .Text($"{(group.Room == "" ? "" : $"{group.Room} - ")}Cabinets ({group.Items.Sum(i => i.Qty)})")
                .FontSize(16)
                .Bold()
                .Italic();

            col.Item()
                .DefaultTextStyle(x => x.FontSize(10))
                .Table(table => {

                    table.ColumnsDefinition(column => {
                        column.ConstantColumn(40);
                        column.ConstantColumn(40);
                        column.RelativeColumn(1);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                    });


                    table.Header(header => {

                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Box Material");
                        header.Cell().ColumnSpan(1).Element(e => applyDefaultCellStyle(e, true, true)).PaddingLeft(5).Text($"{group.BoxCore} - {group.BoxFinish}");
                        header.Cell().ColumnSpan(1).Element(headerCellStyle).Text("Fronts");
                        header.Cell().ColumnSpan(4).Element(e => applyDefaultCellStyle(e, true, true)).PaddingLeft(5).Text(group.Fronts);

                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Fin Material");
                        header.Cell().ColumnSpan(1).Element(e => applyDefaultCellStyle(e, true, true)).PaddingLeft(5).Text($"{group.FinishCore} - {group.FinishFinish}");
                        header.Cell().ColumnSpan(1).Element(headerCellStyle).Text("Paint");
                        header.Cell().ColumnSpan(2).Element(e => applyDefaultCellStyle(e, true, true)).PaddingLeft(5).Text(group.Paint);
                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text(group.Assembled ? "ASSEMBLED" : "NOT ASSEMBLED");

                        new string[] {
                            "#",
                            "Qty",
                            "Description",
                            "Width",
                            "Height",
                            "Depth",
                            "L Side",
                            "R Side"
                        }.ForEach(title =>
                            header.Cell().Element(headerCellStyle).Text(title)
                        );

                    });

                    var cabSide = (CabinetSideType sideType) => sideType switch {
                        CabinetSideType.Unfinished => "",
                        CabinetSideType.Finished => "F",
                        CabinetSideType.ConfirmatFinished => "G",
                        CabinetSideType.AppliedPanel => "AP",
                        CabinetSideType.IntegratedPanel => "IP",
                        _ => sideType.ToString()
                    };

                    foreach (var item in group.Items) {

                        bool containsComments = item.Comments.Any();

                        new Action<IContainer>[] {

                            c => c.AlignCenter().Text(item.Line.ToString()),
                            c => c.AlignCenter().Text(item.Qty.ToString()),
                            c => c.AlignLeft().Text(item.Description),
                            c => c.AlignCenter().Text(text => FormatFraction(text, item.Width, 10)),
                            c => c.AlignCenter().Text(text => FormatFraction(text, item.Height, 10)),
                            c => c.AlignCenter().Text(text => FormatFraction(text, item.Depth, 10)),
                            c => c.AlignCenter().Text(cabSide(item.LeftSide)),
                            c => c.AlignCenter().Text(cabSide(item.RightSide))

                        }.ForEach(action =>
                            action(table.Cell().Element(e => applyDefaultCellStyle(e, !containsComments, true)))
                        );

                        item.Comments.ForEach((comment, index) => {
                            var isLastComment = index == item.Comments.Length - 1;
                            table.Cell()
                                .ColumnSpan(8)
                                .Element(e => applyDefaultCellStyle(e, isLastComment, false))
                                .PaddingLeft(10)
                                .AlignLeft()
                                .Text(comment);
                        });

                    }

                });

        });

    }

    private static void ComposeCabinetPartTable(IContainer container, CabinetPartGroup group) {

        var defaultCellStyle = (IContainer cell)
            => cell.Border(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .AlignMiddle()
                    .PaddingVertical(3)
                    .PaddingHorizontal(3);

        var headerCellStyle = (IContainer cell)
            => cell.Border(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .Background(Colors.Grey.Lighten3)
                    .AlignCenter()
                    .PaddingVertical(3)
                    .PaddingHorizontal(3)
                    .DefaultTextStyle(x => x.Bold());

        container.Column(col => {

            col.Item()
                .PaddingTop(10)
                .PaddingLeft(10)
                .Text($"{(group.Room == "" ? "" : $"{group.Room} - ")}Cabinet Extras ({group.Items.Sum(i => i.Qty)})")
                .FontSize(16)
                .Bold()
                .Italic();

            col.Item()
                .DefaultTextStyle(x => x.FontSize(10))
                .Table(table => {

                    table.ColumnsDefinition(column => {
                        column.ConstantColumn(40);
                        column.ConstantColumn(40);
                        column.ConstantColumn(45);
                        column.ConstantColumn(200);
                    });


                    table.Header(header => {

                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Material");
                        header.Cell().ColumnSpan(2).Element(defaultCellStyle).PaddingLeft(5).Text($"{group.MaterialCore} - {group.MaterialFinish}");

                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Edge Banding");
                        header.Cell().ColumnSpan(2).Element(defaultCellStyle).PaddingLeft(5).Text(group.EdgeBandingFinish);

                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Sku");
                        header.Cell().Element(headerCellStyle).Text("Description");

                    });

                    foreach (var item in group.Items) {

                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Sku);
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description);

                    }

                });

        });

    }

    private static void ComposeClosetPartTable(IContainer container, ClosetPartGroup group) {

        var defaultCellStyle = (IContainer cell)
            => cell.Border(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .AlignMiddle()
                    .PaddingVertical(3)
                    .PaddingHorizontal(3);

        var headerCellStyle = (IContainer cell)
            => cell.Border(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .Background(Colors.Grey.Lighten3)
                    .AlignCenter()
                    .PaddingVertical(3)
                    .PaddingHorizontal(3)
                    .DefaultTextStyle(x => x.Bold());

        container.Column(col => {

            col.Item()
                .PaddingTop(10)
                .PaddingLeft(10)
                .Text($"{(group.Room == "" ? "" : $"{group.Room} - ")}Closet Parts ({group.Items.Sum(i => i.Qty)})")
                .FontSize(16)
                .Bold()
                .Italic();

            col.Item()
                .DefaultTextStyle(x => x.FontSize(10))
                .Table(table => {

                    table.ColumnsDefinition(column => {
                        column.ConstantColumn(40);
                        column.ConstantColumn(40);
                        column.ConstantColumn(45);
                        column.ConstantColumn(200);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                    });


                    table.Header(header => {

                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Material");
                        header.Cell().ColumnSpan(4).Element(defaultCellStyle).PaddingLeft(5).Text($"{group.MaterialCore} - {group.MaterialFinish}");

                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Edge Banding");
                        header.Cell().ColumnSpan(4).Element(defaultCellStyle).PaddingLeft(5).Text($"{group.EdgeBandingMaterial} - {group.EdgeBandingFinish}");

                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Sku");
                        header.Cell().Element(headerCellStyle).Text("Description");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Length");

                    });

                    foreach (var item in group.Items) {

                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Sku);
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description);
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Width, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Length, 10));

                    }

                });

        });

    }

    private static void ComposeZargenDrawerTable(IContainer container, ZargenDrawerGroup group) {

        var defaultCellStyle = (IContainer cell)
            => cell.Border(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .AlignMiddle()
                    .PaddingVertical(3)
                    .PaddingHorizontal(3);

        var headerCellStyle = (IContainer cell)
            => cell.Border(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .Background(Colors.Grey.Lighten3)
                    .AlignCenter()
                    .PaddingVertical(3)
                    .PaddingHorizontal(3)
                    .DefaultTextStyle(x => x.Bold());

        container.Column(col => {

            col.Item()
                .PaddingTop(10)
                .PaddingLeft(10)
                .Text($"{(group.Room == "" ? "" : $"{group.Room} - ")}Closet Parts ({group.Items.Sum(i => i.Qty)})")
                .FontSize(16)
                .Bold()
                .Italic();

            col.Item()
                .DefaultTextStyle(x => x.FontSize(10))
                .Table(table => {

                    table.ColumnsDefinition(column => {
                        column.ConstantColumn(40);
                        column.ConstantColumn(40);
                        column.ConstantColumn(45);
                        column.ConstantColumn(155);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                    });


                    table.Header(header => {

                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Material");
                        header.Cell().ColumnSpan(4).Element(defaultCellStyle).PaddingLeft(5).Text($"{group.MaterialCore} - {group.MaterialFinish}");

                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Edge Banding");
                        header.Cell().ColumnSpan(4).Element(defaultCellStyle).PaddingLeft(5).Text(group.EdgeBandingFinish);

                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Sku");
                        header.Cell().Element(headerCellStyle).Text("Description");
                        header.Cell().Element(headerCellStyle).Text("OpeningWidth");
                        header.Cell().Element(headerCellStyle).Text("Height");
                        header.Cell().Element(headerCellStyle).Text("Depth");

                    });

                    foreach (var item in group.Items) {

                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Sku);
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description);
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.OpeningWidth, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Height, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Depth, 10));

                    }

                });

        });

    }

    private static void ComposeMDFDoorTable(IContainer container, MDFDoorGroup group) {

        var defaultCellStyle = (IContainer cell)
            => cell.Border(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .AlignMiddle()
                    .PaddingVertical(3)
                    .PaddingHorizontal(3);

        var headerCellStyle = (IContainer cell)
            => cell.Border(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .Background(Colors.Grey.Lighten3)
                    .AlignCenter()
                    .PaddingVertical(3)
                    .PaddingHorizontal(3)
                    .DefaultTextStyle(x => x.Bold());

        container.Column(col => {

            col.Item()
                .PaddingTop(10)
                .PaddingLeft(10)
                .Text($"{(group.Room == "" ? "" : $"{group.Room} - ")}MDF Doors ({group.Items.Sum(i => i.Qty)})")
                .FontSize(16)
                .Bold()
                .Italic();

            col.Item()
                .DefaultTextStyle(x => x.FontSize(10))
                .Table(table => {

                    table.ColumnsDefinition(column => {
                        column.ConstantColumn(40);
                        column.ConstantColumn(40);
                        column.ConstantColumn(200);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                    });


                    table.Header(header => {

                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Material");
                        header.Cell().ColumnSpan(3).Element(defaultCellStyle).PaddingLeft(5).Text(group.Material);

                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Style");
                        header.Cell().ColumnSpan(3).Element(defaultCellStyle).PaddingLeft(5).Text(group.Style);

                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Finish");
                        header.Cell().ColumnSpan(3).Element(defaultCellStyle).PaddingLeft(5).Text(group.Finish);

                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Description");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Height");

                    });

                    foreach (var item in group.Items) {

                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description);
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Width, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Height, 10));

                    }


                });

        });

    }

    private static void ComposeFivePieceDoorTable(IContainer container, FivePieceDoorGroup group) {

        var defaultCellStyle = (IContainer cell)
            => cell.Border(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .AlignMiddle()
                    .PaddingVertical(3)
                    .PaddingHorizontal(3);

        var headerCellStyle = (IContainer cell)
            => cell.Border(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .Background(Colors.Grey.Lighten3)
                    .AlignCenter()
                    .PaddingVertical(3)
                    .PaddingHorizontal(3)
                    .DefaultTextStyle(x => x.Bold());

        container.Column(col => {

            col.Item()
                .PaddingTop(10)
                .PaddingLeft(10)
                .Text($"{(group.Room == "" ? "" : $"{group.Room} - ")}Five-Piece Doors ({group.Items.Sum(i => i.Qty)})")
                .FontSize(16)
                .Bold()
                .Italic();

            col.Item()
                .DefaultTextStyle(x => x.FontSize(10))
                .Table(table => {

                    table.ColumnsDefinition(column => {
                        column.ConstantColumn(40);
                        column.ConstantColumn(40);
                        column.ConstantColumn(200);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                    });


                    table.Header(header => {

                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Material");
                        header.Cell().ColumnSpan(3).Element(defaultCellStyle).PaddingLeft(5).Text(group.Material);

                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Description");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Height");

                    });

                    foreach (var item in group.Items) {

                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description);
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Width, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Height, 10));

                    }


                });

        });

    }

    private static void ComposeDoweledDrawerBoxTable(IContainer container, DoweledDrawerBoxGroup group) {

        var defaultCellStyle = (IContainer cell)
            => cell.Border(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .AlignMiddle()
                    .PaddingVertical(3)
                    .PaddingHorizontal(3);

        var headerCellStyle = (IContainer cell)
            => cell.Border(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .Background(Colors.Grey.Lighten3)
                    .AlignCenter()
                    .PaddingVertical(3)
                    .PaddingHorizontal(3)
                    .DefaultTextStyle(x => x.Bold());

        var materialStr = (DoweledDrawerBoxMaterial material)
            => $"{material.Thickness.AsMillimeters():0.00}mm {material.Name}";

        container.Column(col => {

            col.Item()
                .PaddingTop(10)
                .PaddingLeft(10)
                .Text($"{(group.Room == "" ? "" : $"{group.Room} - ")}Doweled Drawer Boxes ({group.Items.Sum(i => i.Qty)})")
                .FontSize(16)
                .Bold()
                .Italic();

            col.Item()
                .DefaultTextStyle(x => x.FontSize(10))
                .Table(table => {

                    table.ColumnsDefinition(column => {
                        column.ConstantColumn(40);
                        column.ConstantColumn(40);
                        column.ConstantColumn(200);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                    });


                    table.Header(header => {

                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Front Material");
                        header.Cell().ColumnSpan(4).Element(defaultCellStyle).PaddingLeft(5).Text(materialStr(group.FrontMaterial));
                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Side Material");
                        header.Cell().ColumnSpan(4).Element(defaultCellStyle).PaddingLeft(5).Text(materialStr(group.SideMaterial));
                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Back Material");
                        header.Cell().ColumnSpan(4).Element(defaultCellStyle).PaddingLeft(5).Text(materialStr(group.BackMaterial));
                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Bottom material");
                        header.Cell().ColumnSpan(4).Element(defaultCellStyle).PaddingLeft(5).Text(materialStr(group.BottomMaterial));
                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Notch");
                        header.Cell().ColumnSpan(4).Element(defaultCellStyle).PaddingLeft(5).Text(group.MachineForUMSlides ? "Notch for UM" : "None");

                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Description");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Height");
                        header.Cell().Element(headerCellStyle).Text("Depth");

                    });


                    foreach (var item in group.Items) {

                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description);
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Width, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Height, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Depth, 10));

                    }

                });

        });

    }

    private static void ComposeDovetailDrawerBoxTable(IContainer container, DovetailDrawerBoxGroup group) {

        var defaultCellStyle = (IContainer cell)
            => cell.Border(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .AlignMiddle()
                    .PaddingVertical(3)
                    .PaddingHorizontal(3);

        var headerCellStyle = (IContainer cell)
            => cell.Border(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .Background(Colors.Grey.Lighten3)
                    .AlignCenter()
                    .PaddingVertical(3)
                    .PaddingHorizontal(3)
                    .DefaultTextStyle(x => x.Bold());

        container.Column(col => {

            col.Item()
                .PaddingTop(10)
                .PaddingLeft(10)
                .Text($"{(group.Room == "" ? "" : $"{group.Room} - ")}Dovetail Drawer Boxes ({group.Items.Sum(i => i.Qty)})")
                .FontSize(16)
                .Bold()
                .Italic();

            col.Item()
                .DefaultTextStyle(x => x.FontSize(10))
                .Table(table => {

                    table.ColumnsDefinition(column => {
                        column.ConstantColumn(40);
                        column.ConstantColumn(40);
                        column.ConstantColumn(200);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                    });


                    table.Header(header => {

                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Material");
                        header.Cell().ColumnSpan(2).Element(defaultCellStyle).PaddingLeft(5).Text(group.Material);
                        header.Cell().ColumnSpan(1).Element(headerCellStyle).Text("Clips");
                        header.Cell().ColumnSpan(3).Element(defaultCellStyle).PaddingLeft(5).Text(group.Clips);

                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Bottoms");
                        header.Cell().ColumnSpan(2).Element(defaultCellStyle).PaddingLeft(5).Text(group.BottomMaterial);
                        header.Cell().ColumnSpan(1).Element(headerCellStyle).Text("Notch");
                        header.Cell().ColumnSpan(3).Element(defaultCellStyle).PaddingLeft(5).Text(group.Notch);

                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Description");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Height");
                        header.Cell().Element(headerCellStyle).Text("Depth");
                        header.Cell().Element(headerCellStyle).Text("Logo");
                        header.Cell().Element(headerCellStyle).Text("Scoop");

                    });


                    foreach (var item in group.Items) {

                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description);
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Width, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Height, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Depth, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Logo ? "Y" : "");
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Scoop ? "Y" : "");

                    }

                });

        });

    }

    private static void ComposeAdditionalItemsTable(IContainer container, IEnumerable<AdditionalItem> items) {

        var defaultCellStyle = (IContainer cell)
            => cell.Border(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .AlignMiddle()
                    .PaddingVertical(5)
                    .PaddingHorizontal(10);

        var headerCellStyle = (IContainer cell)
            => cell.Border(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .Background(Colors.Grey.Lighten3)
                    .PaddingVertical(5)
                    .PaddingHorizontal(10)
                    .DefaultTextStyle(x => x.Bold());

        container.DefaultTextStyle(x => x.FontSize(10))
                .Column(col => {

                    col.Item()
                        .PaddingTop(10)
                        .PaddingLeft(8)
                        .Text("Other")
                        .FontSize(14);

                    col.Item()
                        .Table(table => {

                            table.ColumnsDefinition(column => {
                                column.ConstantColumn(40);
                                column.RelativeColumn();
                            });


                            table.Header(header => {
                                header.Cell().Element(headerCellStyle).Text("Qty");
                                header.Cell().Element(headerCellStyle).Text("Description");
                            });

                            foreach (var item in items) {
                                table.Cell().Element(defaultCellStyle).Text(item.Qty.ToString());
                                table.Cell().Element(defaultCellStyle).Text(item.Description);
                            }

                        });

                });

    }

    private static void ComposeCounterTopTable(IContainer container, IEnumerable<CounterTopItem> items) {

        var defaultCellStyle = (IContainer cell)
            => cell.Border(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .AlignMiddle()
                    .PaddingVertical(5)
                    .PaddingHorizontal(10)
                    .DefaultTextStyle(t => t.ExtraBold());

        var headerCellStyle = (IContainer cell)
            => cell.Border(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .Background(Colors.Grey.Lighten3)
                    .PaddingVertical(5)
                    .PaddingHorizontal(10)
                    .DefaultTextStyle(x => x.Bold());

        var ebText = (EdgeBandingSides eb)
            => eb switch {
                EdgeBandingSides.None => "None",
                EdgeBandingSides.All => "All",
                EdgeBandingSides.OneLong => "1L",
                EdgeBandingSides.OneLongOneShort => "1L1S",
                EdgeBandingSides.OneLongTwoShort => "1L2S",
                EdgeBandingSides.TwoLong => "2L",
                EdgeBandingSides.TwoLongOneShort => "2L1S",
                EdgeBandingSides.OneShort => "1S",
                EdgeBandingSides.TwoShort => "2S",
                _ => "Unknown"
            };

        container.DefaultTextStyle(x => x.FontSize(10))
                .Column(col => {

                    col.Item()
                        .PaddingTop(10)
                        .PaddingLeft(8)
                        .Text("Counter Tops")
                        .FontSize(14);

                    col.Item()
                        .Table(table => {

                            table.ColumnsDefinition(column => {
                                column.ConstantColumn(40);
                                column.RelativeColumn();
                                column.ConstantColumn(55);
                                column.ConstantColumn(55);
                                column.ConstantColumn(40);
                            });


                            table.Header(header => {
                                header.Cell().Element(headerCellStyle).Text("Qty");
                                header.Cell().Element(headerCellStyle).Text("Finish");
                                header.Cell().Element(headerCellStyle).Text("Width");
                                header.Cell().Element(headerCellStyle).Text("Length");
                                header.Cell().Element(headerCellStyle).Text("EB");
                            });

                            foreach (var item in items) {
                                table.Cell().Element(defaultCellStyle).Text(item.Qty.ToString());
                                table.Cell().Element(defaultCellStyle).Text(item.Finish);
                                table.Cell().Element(defaultCellStyle).Text(item.Width.AsMillimeters().ToString("0"));
                                table.Cell().Element(defaultCellStyle).Text(item.Length.AsMillimeters().ToString("0"));
                                table.Cell().Element(defaultCellStyle).Text(ebText(item.EdgeBanding));
                            }

                        });

                });

    }

    private static void ComposeHeader(IContainer container, JobSummary jobSummary) {

        container.Column(col => {

            col.Item().Row(row => {

                (string numberPrefix, string number) = SplitOrderNumber(jobSummary.Number);

                row.RelativeItem()
                    .PaddingRight(10)
                     .Column(col => {
                         col.Item()
                             .Text(t => {
                                 t.Span(numberPrefix)
                                    .FontSize(22)
                                    .Underline()
                                    .ExtraBold();

                                 t.Span($"{number} {jobSummary.Name}")
                                    .FontSize(20)
                                    .SemiBold();
                             });

                         col.Item()
                             .Text(jobSummary.CustomerName)
                             .SemiBold()
                             .FontSize(18);

                         col.Item()
                            .Row(row => {
                                row.ConstantItem(75)
                                    .Text("Ordered")
                                    .FontSize(12);
                                row.AutoItem()
                                    .Text(jobSummary.OrderDate.ToString("MMMM d, yyyy"))
                                    .FontSize(12)
                                    .Italic();
                            });

                         col.Item()
                            .Row(row => {
                                row.ConstantItem(75)
                                    .Text("Released")
                                    .FontSize(12);
                                row.AutoItem()
                                    .Text(jobSummary.ReleaseDate.ToString("MMMM d, yyyy"))
                                    .FontSize(12)
                                    .Italic();
                            });

                         if (jobSummary.DueDate is DateTime dueDate) {
                             col.Item()
                                .Row(row => {
                                    row.ConstantItem(75)
                                        .Text("Due")
                                        .FontSize(12);
                                    row.AutoItem()
                                        .Text(dueDate.ToString("MMMM d, yyyy"))
                                        .FontSize(12)
                                        .Italic();
                                });
                         }
                     });

                if (jobSummary.VendorLogo.Any()) {
                    row.ConstantItem(150)
                        .AlignRight()
                        .Image(jobSummary.VendorLogo);
                }

            });

            col.Item().PaddingTop(15).Text("Customer Note:").FontSize(10).Italic();
            col.Item().PaddingBottom(5).Text(jobSummary.Comment).FontSize(12);

        });

    }

    private static (string prefix, string orderNumber) SplitOrderNumber(string orderNumber) {

        int prefixFirstIndex = orderNumber.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });
        if (prefixFirstIndex == -1) {
            return ("", orderNumber);
        }
        var numberPrefix = orderNumber[..prefixFirstIndex];
        var number = orderNumber[prefixFirstIndex..];

        return (numberPrefix, number);

    }

    private static void FormatFraction(TextDescriptor text, Dimension dim, float fontSize) {

        var fraction = dim.RoundToInchMultiple((double)1 / 64).AsInchFraction();

        if (fraction.N == 0) {
            text.Span("0").FontSize(fontSize);
            return;
        }

        int whole = fraction.N / fraction.D;
        int n = fraction.N - whole * fraction.D;

        if (whole == 0) {

            text.Span(n.ToString()).Superscript().FontSize(fontSize);
            text.Span("/").FontSize(fontSize);
            text.Span(fraction.D.ToString()).FontSize(fontSize - 4);

        } else if (n != 0) {

            text.Span($"{whole} ");
            text.Span(n.ToString()).Superscript().FontSize(fontSize);
            text.Span("/").FontSize(fontSize);
            text.Span(fraction.D.ToString()).FontSize(fontSize - 4);

        } else {

            text.Span(whole.ToString()).FontSize(fontSize);

        }

    }

}
