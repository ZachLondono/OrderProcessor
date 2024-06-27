using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Domain.Extensions;
using OrderExporting.CNC.ReleasePDF.PDFModels;
using OrderExporting.CNC.ReleasePDF.Styling;
using OrderExporting.Shared;
using ApplicationCore.Shared.Settings;

namespace OrderExporting.CNC.ReleasePDF.Services;

public class ReleasePDFDecorator : IDocumentDecorator {

    private readonly PDFConfiguration _config;
    private readonly CoverModel _cover;
    private readonly IEnumerable<PageModel> _pages;

    public ReleasePDFDecorator(PDFConfiguration config, CoverModel cover, IEnumerable<PageModel> pages) {
        _config = config;
        _cover = cover;
        _pages = pages;
    }

    public void Decorate(IDocumentContainer container) {

        if (_cover is not null) {
            container.Page(page => {
                BuildSummary(page, _cover, _config);
            });
        }


        string sectionId = $"Patterns {Guid.NewGuid()}";
        foreach (var data in _pages) {

            container.Page(page => {
                BuildPage(page, data, _config, sectionId);
            });
        }

    }

    private static void BuildSummary(PageDescriptor page, CoverModel summary, PDFConfiguration config) {

        page.Size(PageSizes.Letter);
        page.Margin(2.54f, Unit.Centimetre);
        page.PageColor(Colors.White);

        var pageHeaderStyle = config.HeaderStyle;
        page.Header()
            .AlignCenter()
            .Column(col => {

                col.Item().AlignCenter().Text(summary.Title).WithStyle(pageHeaderStyle);

            });

        page.Footer()
            .Row(row => {

                row.RelativeItem()
                    .AlignLeft()
                    .Text(summary.TimeStamp.ToString("g"))
                    .FontSize(8);

                row.RelativeItem()
                    .AlignRight()
                    .Text(summary.ApplicationVersion)
                    .FontSize(8);

            });

        page.Content().Column(c => {

            var titleStyle = config.TitleStyle;
            var tableHeaderStyle = config.TableHeaderStyle;
            var tableCellStyle = config.TableCellStyle;

            c.Spacing(20);

            c.Item().Table(t => {
                t.ColumnsDefinition(c => {
                    c.ConstantColumn(config.InfoLabelColumnWidth);
                    c.RelativeColumn();
                });

                foreach (var item in summary.Info) {
                    t.Cell().BorderTop(0.5f)
                            .BorderLeft(0.5f)
                            .BorderBottom(0.5f)
                            .Background(Colors.Grey.Lighten3)
                            .PaddingLeft(5)
                            .AlignLeft()
                            .Text(item.Key)
                            .WithStyle(tableHeaderStyle);

                    t.Cell().BorderLeft(0.5f)
                            .BorderRight(0.5f)
                            .BorderTop(0.5f)
                            .BorderBottom(0.5f)
                            .PaddingLeft(5)
                            .AlignLeft()
                            .Text(item.Value)
                            .WithStyle(tableCellStyle);
                }
            });


            foreach (var tabledata in summary.Tables) {

                c.Item()
                    .Column(c => {
                        c.Item().Text(tabledata.Title).WithStyle(titleStyle);
                        if (tabledata.Content.Count == 0) {
                            c.Item().Text("no data").Italic();
                        } else {
                            c.Item().Table(t => {
                                BuildTable(t, tabledata, config);
                            });
                        }
                    });

            }

        });

    }

    private static void BuildPage(PageDescriptor page, PageModel data, PDFConfiguration config, string sectionId) {

        page.Size(PageSizes.Letter);
        page.MarginHorizontal(2, Unit.Centimetre);
        page.MarginVertical(5, Unit.Millimetre);
        page.PageColor(Colors.White);

        var headerStyle = config.HeaderStyle;
        page.Header()
            .AlignCenter()
            .Text(data.Header)
            .WithStyle(headerStyle);

        var titleStyle = config.TitleStyle;

        page.Content()
            .Section(sectionId)
            .PaddingVertical(1, Unit.Centimetre)
            .Column(x => {

                if (data.ImageData.Length != 0) {
                    x.Item()
                        .AlignCenter()
                        .MaxHeight(350)
                        .Image(data.ImageData, ImageScaling.FitArea);
                }

                x.Item().AlignCenter()
                        .PaddingVertical(5)
                        .Text(data.Subtitle)
                        .WithStyle(titleStyle);

                x.Item().AlignCenter()
                        .PaddingBottom(10)
                        .Table(progTable => {

                            progTable.ColumnsDefinition(cols => {
                                data.MachinePrograms.ForEach(_ => cols.ConstantColumn(100));
                            });

                            data.MachinePrograms
                                .ForEach(r =>
                                    progTable.Cell()
                                            .Border(0.25f)
                                            .Background(Colors.Grey.Lighten3)
                                            .AlignCenter()
                                            .Text(r.Key)
                                            .WithStyle(titleStyle)
                                );

                            data.MachinePrograms
                                .ForEach(r =>
                                    progTable.Cell()
                                            .Border(0.25f)
                                            .AlignCenter()
                                            .Text(r.Value.Face5Program)
                                            .WithStyle(titleStyle)
                                );

                            if (data.MachinePrograms.First().Value.Face5Program is not null)
                                data.MachinePrograms.ForEach(r =>
                                    progTable.Cell()
                                            .Border(0.25f)
                                            .AlignCenter()
                                            .Text(r.Value.Face6Program)
                                            .WithStyle(titleStyle)
                                );

                        });
                if (data.Parts.Content.Any()) {
                    x.Item().Column(c => {
                        c.Item().AlignLeft()
                            .Text(data.Parts.Title)
                            .WithStyle(titleStyle);
                        c.Item().Table(t => {
                            BuildTable(t, data.Parts, config);
                        });
                    });
                }

            });

        page.Footer()
            .Column(c => {
                c.Item()
                .AlignCenter()
                .Text(x => {
                    x.DefaultTextStyle(x => x.FontSize(10));
                    x.Span("Page ");
                    x.PageNumberWithinSection(sectionId);
                    x.Span(" of ");
                    x.TotalPagesWithinSection(sectionId);
                });
                c.Item()
                .AlignCenter()
                .Text(data.TimeStamp.ToString("g"))
                .FontSize(10);
            });

    }

    private static void BuildTable(TableDescriptor table, Table data, PDFConfiguration config) {

        var headers = data.Content.SelectMany(r => r.Keys).Distinct();

        table.ColumnsDefinition(c => {
            foreach (var key in headers) {
                if (data.ColumnWidths.TryGetValue(key, out var constantWidth)) {
                    c.ConstantColumn(constantWidth);
                } else {
                    c.RelativeColumn();
                }
            }
        });

        var headerStyle = config.TableHeaderStyle;
        table.Header(h => {
            foreach (var key in headers)
                h.Cell()
                    .BorderTop(0.5f)
                    .BorderLeft(0.5f)
                    .BorderRight(0.5f)
                    .BorderBottom(0.5f)
                    .Background(Colors.Grey.Lighten3)
                    .AlignCenter()
                    .AlignMiddle()
                    .Text(key)
                    .WithStyle(headerStyle);

        });

        var cellStyle = config.TableCellStyle;
        foreach (var row in data.Content) {
            foreach (var key in headers) {
                if (!row.TryGetValue(key, out string? value)) value = "";
                table.Cell()
                    .BorderLeft(0.5f)
                    .BorderRight(0.5f)
                    .BorderBottom(0.5f)
                    .AlignCenter()
                    .AlignMiddle()
                    .Text(value)
                    .WithStyle(cellStyle);
            }
        }

    }

}
