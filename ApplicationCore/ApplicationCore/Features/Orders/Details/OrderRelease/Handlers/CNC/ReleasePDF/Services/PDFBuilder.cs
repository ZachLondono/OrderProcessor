using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Configuration;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.PDFModels;
using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Styling;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using BarcodeLib;
using MoreLinq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.ReleasePDF.Services;

internal class ReleasePDFDecorator : IDocumentDecorator {

    private readonly PDFConfiguration _config;
    private readonly CoverModel _cover;
    private readonly IEnumerable<PageModel> _pages;

    public ReleasePDFDecorator(PDFConfiguration config, CoverModel cover, IEnumerable<PageModel> pages) {
        _config = config;
        _cover = cover;
        _pages = pages;
    }

    public Task Decorate(Order order, IDocumentContainer container) {

        if (_cover is not null) {
            container.Page(page => {
                BuildSummary(page, _cover, _config);
            });
        }

        foreach (var data in _pages) {
            container.Page(page => {
                BuildPage(page, data, _config);
            });
        }

        return Task.CompletedTask;

    }

    private static void BuildSummary(PageDescriptor page, CoverModel summary, PDFConfiguration config) {

        page.Size(PageSizes.A4);
        page.Margin(2.54f, Unit.Centimetre);
        page.PageColor(Colors.White);

        var pageHeaderStyle = config.HeaderStyle;
        page.Header()
            .AlignCenter()
            .Column(col => {

                col.Item().AlignCenter().Text(summary.Title).WithStyle(pageHeaderStyle);

                if (summary.WorkOrderId != "") {

                    var barcode = new Barcode();
                    var img = barcode.Encode(TYPE.CODE128B, summary.WorkOrderId, 300, 50);

                    using var ms = new MemoryStream();
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    var imgdata = ms.ToArray();

                    col.Item()
                        .AlignCenter()
                        .PaddingVertical(5)
                        .Width(2, Unit.Inch)
                        .Image(imgdata);
                }

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

    private static void BuildPage(PageDescriptor page, PageModel data, PDFConfiguration config) {

        page.Size(PageSizes.A4);
        page.MarginHorizontal(2, Unit.Centimetre);
        page.PageColor(Colors.White);

        var headerStyle = config.HeaderStyle;
        page.Header()
            .AlignCenter()
            .Text(data.Header)
            .WithStyle(headerStyle);

        var titleStyle = config.TitleStyle;
        page.Content()
            .PaddingVertical(1, Unit.Centimetre)
            .Column(x => {

                if (data.ImageData.Length != 0)
                    x.Item().Image(data.ImageData);

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

                x.Item().Column(c => {
                    c.Item().AlignLeft()
                        .Text(data.Parts.Title)
                        .WithStyle(titleStyle);
                    c.Item().Table(t => {
                        BuildTable(t, data.Parts, config);
                    });
                });

            });

        page.Footer()
            .Column(c => {
                c.Item()
                .AlignCenter()
                .Text(x => {
                    x.DefaultTextStyle(x => x.FontSize(10));
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
                c.Item()
                .AlignCenter()
                .Text(DateTime.Today.ToShortDateString())
                .FontSize(10);
            });

    }

    private static void BuildTable(TableDescriptor table, Table data, PDFConfiguration config) {

        var headers = data.Content.SelectMany(r => r.Keys).Distinct();

        table.ColumnsDefinition(c => {
            foreach (var key in headers) c.RelativeColumn();
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
