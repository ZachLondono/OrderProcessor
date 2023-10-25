using ApplicationCore.Shared;
using ApplicationCore.Shared.Domain;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.DovetailDBPackingList;

public class DovetailDBPackingListDecorator : IDocumentDecorator {

    public DovetailDrawerBoxPackingList Data { get; set; } = new();

    public void Decorate(IDocumentContainer container) {

        container.Page(page => {

            page.Size(PageSizes.Letter);
            page.Margin(1, Unit.Centimetre);

            page.Content()
                .Column(column => {

                column.Item()
                    .AlignCenter()
                    .PaddingBottom(10)
                    .Text("Dovetail Drawer Boxes")
                    .FontSize(36)
                    .Bold();

                ComposeHeader(column.Item(), Data);
                ComposeTable(column.Item(), Data.Items);

            });

        });

    }

    private static void ComposeHeader(IContainer container, DovetailDrawerBoxPackingList data) {

        container.PaddingBottom(10).Row(row => {

            row.RelativeItem()
                .Column(companyCol => {

                    companyCol.Item()
                              .Element(e => ComposeCompanyInfo(e, "From:", data.Vendor));

                    companyCol.Item()
                            .PaddingTop(15)
                            .Element(e => ComposeCompanyInfo(e, "To:", data.Customer));

                });

            row.ConstantItem(150)
                .AlignRight()
                .Column(infoCol => {

                    infoCol.Item()
                            .Row(dateRow => {

                                dateRow.AutoItem()
                                        .PaddingRight(5)
                                        .Column(infoCol => {

                                            infoCol.Item()
                                                .Text("Date")
                                                .FontSize(12)
                                                .SemiBold();

                                        });

                                dateRow.ConstantItem(118)
                                        .Text(data.OrderDate.ToString("MMMM d, yyyy"))
                                        .Italic()
                                        .FontSize(12);

                            });

                    infoCol.Item()
                            .PaddingTop(25)
                            .Row(trackingNumRow => {

                                trackingNumRow.AutoItem()
                                                .PaddingRight(5)
                                                .Text("Tracking #")
                                                .FontSize(12)
                                                .SemiBold();

                                trackingNumRow.ConstantItem(90)
                                                .Text(data.OrderNumber)
                                                .FontSize(12);

                            });

                    infoCol.Item()
                            .Row(nameRow => {

                                nameRow.AutoItem()
                                        .PaddingRight(5)
                                        .Text("Name")
                                        .FontSize(12)
                                        .SemiBold();

                                nameRow.ConstantItem(113)
                                        .Text(data.OrderName)
                                        .FontSize(12);

                            });

                });

        });

    }

    public static void ComposeCompanyInfo(IContainer container, string label, Company company) {

        container.Row(row => {

            row.ConstantItem(70)
                .AlignRight()
                .PaddingLeft(15)
                .PaddingRight(15)
                .Text(label)
                .Italic()
                .FontSize(16);

            row.AutoItem().Column(col => {

                col.Item().Text(company.Name).SemiBold().FontSize(16);
                if (!string.IsNullOrWhiteSpace(company.Line1))
                    col.Item().Text(company.Line1).FontSize(12);
                if (!string.IsNullOrWhiteSpace(company.Line2))
                    col.Item().Text(company.Line2).FontSize(12);
                if (!string.IsNullOrWhiteSpace(company.Line3))
                    col.Item().Text(company.Line3).FontSize(12);
                if (!string.IsNullOrWhiteSpace(company.Line4))
                    col.Item().Text(company.Line4).FontSize(12);

            });

        });

    }

    private static void ComposeTable(IContainer container, IEnumerable<DovetailDrawerBox> boxes) {

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
                .AlignCenter()
                .PaddingTop(20)
                .DefaultTextStyle(x => x.FontSize(10))
                .Table(table => {

                    table.ColumnsDefinition(column => {
                        column.ConstantColumn(40);
                        column.ConstantColumn(40);
                        column.RelativeColumn();
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                    });


                    table.Header(header => {

                        header.Cell().ColumnSpan(3).PaddingLeft(10).Text($"{boxes.Sum(i => i.Qty)} Boxes").FontSize(16).Bold().Italic();
                        header.Cell().ColumnSpan(3).Element(headerCellStyle).Text("Inches");
                        header.Cell().ColumnSpan(3).Element(headerCellStyle).Text("Millimeters");

                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Description");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Height");
                        header.Cell().Element(headerCellStyle).Text("Depth");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Height");
                        header.Cell().Element(headerCellStyle).Text("Depth");

                    });

                    foreach (var item in boxes) {
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description);
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Width, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Height, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Depth, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Width.AsMillimeters().ToString("0"));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Height.AsMillimeters().ToString("0"));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Depth.AsMillimeters().ToString("0"));
                    }

                });

        });    }

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
