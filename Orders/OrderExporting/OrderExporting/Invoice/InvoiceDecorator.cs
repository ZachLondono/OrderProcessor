using Domain.Companies;
using Domain.Orders.Entities;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.Entities.Products.Closets;
using Domain.Orders.Entities.Products.Doors;
using Domain.Orders.Entities.Products.DrawerBoxes;
using Domain.ValueObjects;
using OrderExporting.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OrderExporting.Invoice;

public class InvoiceDecorator(Invoice inovice) : IDocumentDecorator {

    private readonly Invoice? _invoice = inovice;

    public void Decorate(IDocumentContainer container) {

        if (_invoice is null) {
            return;
        }

        container.Page(page => {

            page.Size(PageSizes.Letter);
            page.Margin(1, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(20));

            page.Content()
                .Column(column => {

                    column.Item()
                            .AlignCenter()
                            .PaddingBottom(10)
                            .Text("Invoice")
                            .FontSize(36)
                            .Bold();

                    ComposeHeader(column.Item(), _invoice);

                    if (_invoice.Cabinets.Any()) {
                        ComposeCabinetTable(column.Item(), _invoice.Cabinets);
                    }

                    if (_invoice.CabinetParts.Any()) {
                        ComposeCabinetPartTable(column.Item(), _invoice.CabinetParts);
                    }

                    if (_invoice.ClosetParts.Any()) {
                        ComposeClosetPartTable(column.Item(), _invoice.ClosetParts);
                    }

                    if (_invoice.ZargenDrawers.Any()) {
                        ComposeZargenDrawerTable(column.Item(), _invoice.ZargenDrawers);
                    }

                    if (_invoice.MDFDoors.Any()) {
                        ComposeMDFDoorTable(column.Item(), _invoice.MDFDoors);
                    }

                    if (_invoice.FivePieceDoors.Any()) {
                        ComposeFivePieceDoorTable(column.Item(), _invoice.FivePieceDoors);
                    }

                    if (_invoice.DovetailDrawerBoxes.Any()) {
                        ComposeDovetailDrawerBoxTable(column.Item(), _invoice.DovetailDrawerBoxes);
                    }

                    if (_invoice.DoweledDrawerBoxes.Any()) {
                        ComposeDoweledDrawerBoxTable(column.Item(), _invoice.DoweledDrawerBoxes);
                    }

                    if (_invoice.AdditionalItems.Any()) {
                        ComposeAdditionalItemTable(column.Item(), _invoice.AdditionalItems);
                    }

                });

        });

    }

    private static void ComposeDetails(ColumnDescriptor column, Invoice invoice) {

        column.Item()
            .PaddingRight(24)
            .Row(row => {

                row.RelativeItem();

                row.ConstantItem(173)
                    .PaddingRight(30)
                    .Table(table => {

                        table.ColumnsDefinition(cols => {
                            cols.ConstantColumn(75);
                            cols.RelativeColumn();
                        });

                        table.Cell().AlignRight().PaddingRight(20).Text("Sub Total:").FontSize(12).SemiBold();
                        table.Cell().AlignRight().Text(invoice.SubTotal.ToString("$0.00")).FontSize(12);
                        table.Cell().AlignRight().PaddingRight(20).Text("Sales Tax:").FontSize(12).SemiBold();
                        table.Cell().AlignRight().Text(invoice.SalesTax.ToString("$0.00")).FontSize(12);
                        table.Cell().BorderBottom(1).AlignRight().PaddingBottom(2).PaddingRight(20).Text("Shipping:").FontSize(12).SemiBold();
                        table.Cell().BorderBottom(1).AlignRight().PaddingBottom(2).Text(invoice.Shipping.ToString("$0.00")).FontSize(12);
                        table.Cell().AlignRight().PaddingTop(2).PaddingRight(20).Text("Total:").FontSize(14).Bold();
                        table.Cell().AlignRight().PaddingTop(2).Text(invoice.Total.ToString("$0.00")).FontSize(14);

                    });

            });

    }

    private static void ComposeHeader(IContainer container, Invoice invoice) {

        container.PaddingBottom(10).Row(row => {

            row.RelativeItem()
                .Column(companyCol => {

                    companyCol.Item()
                              .Element(e => ComposeCompanyInfo(e, "From:", invoice.Vendor));

                    companyCol.Item()
                            .PaddingTop(15)
                            .Element(e => ComposeCompanyInfo(e, "To:", invoice.Customer));

                });

            row.ConstantItem(197)
                .AlignRight()
                .Column(infoCol => {

                    infoCol.Item()
                            .Row(dateRow => {

                                dateRow.ConstantItem(48)
                                        .Column(infoCol => {

                                            infoCol.Item()
                                                .AlignRight()
                                                .Text("Date")
                                                .FontSize(12)
                                                .SemiBold();

                                        });

                                dateRow.ConstantItem(118)
                                        .PaddingLeft(5)
                                        .Text(invoice.Date.ToString("MMMM d, yyyy"))
                                        .Italic()
                                        .FontSize(12);

                            });

                    infoCol.Item()
                            .PaddingTop(25)
                            .PaddingBottom(25)
                            .Table(table => {

                                table.ColumnsDefinition(cols => {
                                    cols.ConstantColumn(48);
                                    cols.ConstantColumn(148);
                                });


                                table.Cell().AlignRight().Text("Invoice #").FontSize(12).SemiBold();
                                table.Cell().PaddingLeft(5).Text(invoice.OrderNumber).FontSize(12);
                                table.Cell().AlignRight().Text("Job").FontSize(12).SemiBold();
                                table.Cell().PaddingLeft(5).Text(invoice.OrderName).FontSize(12);
                                table.Cell().AlignRight().Text("Terms").FontSize(12).SemiBold();
                                table.Cell().PaddingLeft(5).Text("COD").FontSize(12);
                            });

                    ComposeDetails(infoCol, invoice);

                });

        });

    }

    private static void ComposeCompanyInfo(IContainer container, string label, Company company) {

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

    private static void ComposeCabinetTable(IContainer container, IEnumerable<CabinetItem> items) {
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
                .Text($"Cabinets ({items.Sum(i => i.Qty)})")
                .FontSize(16)
                .Bold()
                .Italic();

            col.Item()
                .DefaultTextStyle(x => x.FontSize(10))
                .Table(table => {

                    table.ColumnsDefinition(column => {
                        column.ConstantColumn(40);
                        column.ConstantColumn(40);
                        column.RelativeColumn();
                        column.ConstantColumn(55);
                        column.ConstantColumn(55);
                        column.ConstantColumn(45);
                        column.ConstantColumn(55);
                        column.ConstantColumn(55);
                    });


                    table.Header(header => {

                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Description");
                        header.Cell().Element(headerCellStyle).Text("Height");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Depth");
                        header.Cell().Element(headerCellStyle).Text("Unit $");
                        header.Cell().Element(headerCellStyle).Text("Ext $");

                    });

                    foreach (var item in items) {
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Height, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Width, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Depth, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.UnitPrice.ToString("0.00"));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text((item.UnitPrice * item.Qty).ToString("$0.00"));
                    }

                    table.Cell().ColumnSpan(6).PaddingVertical(3).PaddingRight(5).AlignRight().Text("Sub Total: ").SemiBold();
                    table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten3).PaddingVertical(3).PaddingRight(10).AlignCenter().Text(items.Sum(i => i.Qty * i.UnitPrice).ToString("$0.00"));

                });

        });
    }

    private static void ComposeCabinetPartTable(IContainer container, IEnumerable<CabinetPartItem> items) {
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
                .Text($"Cabinet Extras ({items.Sum(i => i.Qty)})")
                .FontSize(16)
                .Bold()
                .Italic();

            col.Item()
                .DefaultTextStyle(x => x.FontSize(10))
                .Table(table => {

                    table.ColumnsDefinition(column => {
                        column.ConstantColumn(40);
                        column.ConstantColumn(40);
                        column.RelativeColumn();
                        column.ConstantColumn(55);
                        column.ConstantColumn(55);
                    });


                    table.Header(header => {

                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Description");
                        header.Cell().Element(headerCellStyle).Text("Unit $");
                        header.Cell().Element(headerCellStyle).Text("Ext $");

                    });

                    foreach (var item in items) {
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.UnitPrice.ToString("0.00"));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text((item.UnitPrice * item.Qty).ToString("$0.00"));
                    }

                    table.Cell().ColumnSpan(3).PaddingVertical(3).PaddingRight(5).AlignRight().Text("Sub Total: ").SemiBold();
                    table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten3).PaddingVertical(3).PaddingRight(10).AlignCenter().Text(items.Sum(i => i.Qty * i.UnitPrice).ToString("$0.00"));

                });

        });
    }

    private static void ComposeClosetPartTable(IContainer container, IEnumerable<ClosetPartItem> items) {
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
                .Text($"Closet Parts ({items.Sum(i => i.Qty)})")
                .FontSize(16)
                .Bold()
                .Italic();

            col.Item()
                .DefaultTextStyle(x => x.FontSize(10))
                .Table(table => {

                    table.ColumnsDefinition(column => {
                        column.ConstantColumn(40);
                        column.ConstantColumn(40);
                        column.RelativeColumn();
                        column.ConstantColumn(55);
                        column.ConstantColumn(55);
                        column.ConstantColumn(55);
                        column.ConstantColumn(55);
                    });


                    table.Header(header => {

                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Description");
                        header.Cell().Element(headerCellStyle).Text("Length");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Unit $");
                        header.Cell().Element(headerCellStyle).Text("Ext $");

                    });

                    foreach (var item in items) {
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Length, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Width, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.UnitPrice.ToString("0.00"));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text((item.UnitPrice * item.Qty).ToString("$0.00"));
                    }

                    table.Cell().ColumnSpan(5).PaddingVertical(3).PaddingRight(5).AlignRight().Text("Sub Total: ").SemiBold();
                    table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten3).PaddingVertical(3).PaddingRight(10).AlignCenter().Text(items.Sum(i => i.Qty * i.UnitPrice).ToString("$0.00"));

                });

        });
    }

    private static void ComposeZargenDrawerTable(IContainer container, IEnumerable<ZargenDrawerItem> items) {
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
                .Text($"Zargen Drawers ({items.Sum(i => i.Qty)})")
                .FontSize(16)
                .Bold()
                .Italic();

            col.Item()
                .DefaultTextStyle(x => x.FontSize(10))
                .Table(table => {

                    table.ColumnsDefinition(column => {
                        column.ConstantColumn(40);
                        column.ConstantColumn(40);
                        column.RelativeColumn();
                        column.ConstantColumn(55);
                        column.ConstantColumn(55);
                        column.ConstantColumn(55);
                        column.ConstantColumn(55);
                        column.ConstantColumn(55);
                    });


                    table.Header(header => {

                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Description");
                        header.Cell().Element(headerCellStyle).Text("OpeningWidth");
                        header.Cell().Element(headerCellStyle).Text("Height");
                        header.Cell().Element(headerCellStyle).Text("Depth");
                        header.Cell().Element(headerCellStyle).Text("Unit $");
                        header.Cell().Element(headerCellStyle).Text("Ext $");

                    });

                    foreach (var item in items) {
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.OpeningWidth, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Height, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Depth, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.UnitPrice.ToString("0.00"));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text((item.UnitPrice * item.Qty).ToString("$0.00"));
                    }

                    table.Cell().ColumnSpan(5).PaddingVertical(3).PaddingRight(5).AlignRight().Text("Sub Total: ").SemiBold();
                    table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten3).PaddingVertical(3).PaddingRight(10).AlignCenter().Text(items.Sum(i => i.Qty * i.UnitPrice).ToString("$0.00"));

                });

        });
    }

    private static void ComposeMDFDoorTable(IContainer container, IEnumerable<MDFDoorItem> items) {

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
                .Text($"MDF Doors ({items.Sum(i => i.Qty)})")
                .FontSize(16)
                .Bold()
                .Italic();

            col.Item()
                .DefaultTextStyle(x => x.FontSize(10))
                .Table(table => {

                    table.ColumnsDefinition(column => {
                        column.ConstantColumn(40);
                        column.ConstantColumn(40);
                        column.RelativeColumn();
                        column.ConstantColumn(55);
                        column.ConstantColumn(55);
                        column.ConstantColumn(55);
                        column.ConstantColumn(55);
                    });


                    table.Header(header => {

                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Description");
                        header.Cell().Element(headerCellStyle).Text("Height");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Unit $");
                        header.Cell().Element(headerCellStyle).Text("Ext $");

                    });

                    foreach (var item in items) {
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Height, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Width, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.UnitPrice.ToString("0.00"));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text((item.UnitPrice * item.Qty).ToString("$0.00"));
                    }

                    table.Cell().ColumnSpan(5).PaddingVertical(3).PaddingRight(5).AlignRight().Text("Sub Total: ").SemiBold();
                    table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten3).PaddingVertical(3).PaddingRight(10).AlignCenter().Text(items.Sum(i => i.Qty * i.UnitPrice).ToString("$0.00"));

                });

        });

    }

    private static void ComposeFivePieceDoorTable(IContainer container, IEnumerable<FivePieceDoorItem> items) {

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
                .Text($"Five-Piece Doors ({items.Sum(i => i.Qty)})")
                .FontSize(16)
                .Bold()
                .Italic();

            col.Item()
                .DefaultTextStyle(x => x.FontSize(10))
                .Table(table => {

                    table.ColumnsDefinition(column => {
                        column.ConstantColumn(40);
                        column.ConstantColumn(40);
                        column.RelativeColumn();
                        column.ConstantColumn(55);
                        column.ConstantColumn(55);
                        column.ConstantColumn(55);
                        column.ConstantColumn(55);
                    });


                    table.Header(header => {

                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Description");
                        header.Cell().Element(headerCellStyle).Text("Height");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Unit $");
                        header.Cell().Element(headerCellStyle).Text("Ext $");

                    });

                    foreach (var item in items) {
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Height, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Width, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.UnitPrice.ToString("0.00"));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text((item.UnitPrice * item.Qty).ToString("$0.00"));
                    }

                    table.Cell().ColumnSpan(5).PaddingVertical(3).PaddingRight(5).AlignRight().Text("Sub Total: ").SemiBold();
                    table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten3).PaddingVertical(3).PaddingRight(10).AlignCenter().Text(items.Sum(i => i.Qty * i.UnitPrice).ToString("$0.00"));

                });

        });

    }

    private static void ComposeDovetailDrawerBoxTable(IContainer container, IEnumerable<DovetailDrawerBoxItem> items) {

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
                .Text($"Dovetail Drawer Boxes ({items.Sum(i => i.Qty)})")
                .FontSize(16)
                .Bold()
                .Italic();

            col.Item()
                .DefaultTextStyle(x => x.FontSize(10))
                .Table(table => {

                    table.ColumnsDefinition(column => {
                        column.ConstantColumn(40);
                        column.ConstantColumn(40);
                        column.RelativeColumn();
                        column.ConstantColumn(55);
                        column.ConstantColumn(55);
                        column.ConstantColumn(45);
                        column.ConstantColumn(55);
                        column.ConstantColumn(55);
                    });


                    table.Header(header => {

                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Description");
                        header.Cell().Element(headerCellStyle).Text("Height");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Depth");
                        header.Cell().Element(headerCellStyle).Text("Unit $");
                        header.Cell().Element(headerCellStyle).Text("Ext $");

                    });

                    foreach (var item in items) {
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Height, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Width, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Depth, 10));

                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.UnitPrice.ToString("0.00"));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text((item.UnitPrice * item.Qty).ToString("$0.00"));
                    }

                    table.Cell().ColumnSpan(6).PaddingVertical(3).PaddingRight(5).AlignRight().Text("Sub Total: ").SemiBold();
                    table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten3).PaddingVertical(3).PaddingRight(10).AlignCenter().Text(items.Sum(i => i.Qty * i.UnitPrice).ToString("$0.00"));

                });

        });

    }

    private static void ComposeDoweledDrawerBoxTable(IContainer container, IEnumerable<DoweledDrawerBoxItem> items) {

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
                .Text($"Doweled Drawer Boxes ({items.Sum(i => i.Qty)})")
                .FontSize(16)
                .Bold()
                .Italic();

            col.Item()
                .DefaultTextStyle(x => x.FontSize(10))
                .Table(table => {

                    table.ColumnsDefinition(column => {
                        column.ConstantColumn(40);
                        column.ConstantColumn(40);
                        column.RelativeColumn();
                        column.ConstantColumn(55);
                        column.ConstantColumn(55);
                        column.ConstantColumn(45);
                        column.ConstantColumn(55);
                        column.ConstantColumn(55);
                    });


                    table.Header(header => {

                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Description");
                        header.Cell().Element(headerCellStyle).Text("Height");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Depth");
                        header.Cell().Element(headerCellStyle).Text("Unit $");
                        header.Cell().Element(headerCellStyle).Text("Ext $");

                    });

                    foreach (var item in items) {
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Height, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Width, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Depth, 10));

                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.UnitPrice.ToString("0.00"));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text((item.UnitPrice * item.Qty).ToString("$0.00"));
                    }

                    table.Cell().ColumnSpan(6).PaddingVertical(3).PaddingRight(5).AlignRight().Text("Sub Total: ").SemiBold();
                    table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten3).PaddingVertical(3).PaddingRight(10).AlignCenter().Text(items.Sum(i => i.Qty * i.UnitPrice).ToString("$0.00"));

                });

        });

    }

    private static void ComposeAdditionalItemTable(IContainer container, IEnumerable<AdditionalItem> items) {

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
                .Text($"Additional Items ({items.Sum(i => i.Qty)})")
                .FontSize(16)
                .Bold()
                .Italic();

            col.Item()
                .DefaultTextStyle(x => x.FontSize(10))
                .Table(table => {

                    table.ColumnsDefinition(column => {
                        column.ConstantColumn(40);
                        column.ConstantColumn(40);
                        column.RelativeColumn();
                        column.ConstantColumn(55);
                        column.ConstantColumn(55);
                    });


                    table.Header(header => {

                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Description");
                        header.Cell().Element(headerCellStyle).Text("Unit $");
                        header.Cell().Element(headerCellStyle).Text("Ext $");

                    });

                    foreach (var item in items) {
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.UnitPrice.ToString("0.00"));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text((item.UnitPrice * item.Qty).ToString("$0.00"));
                    }

                    table.Cell().ColumnSpan(3).PaddingVertical(3).PaddingRight(5).AlignRight().Text("Sub Total: ").SemiBold();
                    table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten3).PaddingVertical(3).PaddingRight(10).AlignCenter().Text(items.Sum(i => i.UnitPrice).ToString("$0.00"));

                });

        });

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