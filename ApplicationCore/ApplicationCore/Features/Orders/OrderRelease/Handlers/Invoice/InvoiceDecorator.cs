using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Cabinets;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Closets;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Doors;
using ApplicationCore.Features.Orders.Shared.Domain.Products.DrawerBoxes;
using ApplicationCore.Shared.Domain;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.Invoice;

internal class InvoiceDecorator : IInvoiceDecorator {

    private Invoice? invoice = null;

    private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerByIdAsync;
    private readonly CompanyDirectory.GetVendorByIdAsync _getVendorByIdAsync;

    public InvoiceDecorator(CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync, CompanyDirectory.GetVendorByIdAsync getVendorByIdAsync) {
        _getCustomerByIdAsync = getCustomerByIdAsync;
        _getVendorByIdAsync = getVendorByIdAsync;
    }

    public async Task AddData(Order order) {
        invoice = await CreateInvoiceModel(order);
    }

    public void Decorate(IDocumentContainer container) {

        if (invoice is null) {
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

                    ComposeHeader(column.Item(), invoice);

                    if (invoice.Cabinets.Any()) {
                        ComposeCabinetTable(column.Item(), invoice.Cabinets);
                    }

                    if (invoice.ClosetParts.Any()) {
                        ComposeClosetPartTable(column.Item(), invoice.ClosetParts);
                    }

                    if (invoice.Doors.Any()) {
                        ComposeDoorTable(column.Item(), invoice.Doors);
                    }

                    if (invoice.DrawerBoxes.Any()) {
                        ComposeDrawerBoxTable(column.Item(), invoice.DrawerBoxes);
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

    private static void ComposeDoorTable(IContainer container, IEnumerable<DoorItem> items) {

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
                .Text($"Doors ({items.Sum(i => i.Qty)})")
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

    private static void ComposeDrawerBoxTable(IContainer container, IEnumerable<DrawerBoxItem> items) {

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
                .Text($"Drawer Boxes ({items.Sum(i => i.Qty)})")
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

    private async Task<Invoice> CreateInvoiceModel(Order order) {

        var vendor = await _getVendorByIdAsync(order.VendorId);
        var customer = await _getCustomerByIdAsync(order.CustomerId);

        return new Invoice() {
            OrderNumber = order.Number,
            OrderName = order.Name,
            Date = DateTime.Today,
            SubTotal = order.SubTotal,
            SalesTax = order.Tax,
            Shipping = order.Shipping.Price,
            Total = order.Total,
            Terms = "COD",
            Discount = 0M,
            Vendor = new() {
                Name = vendor?.Name ?? "",
                Line1 = vendor?.Address.Line1 ?? "",
                Line2 = vendor?.Address.Line2 ?? "",
                Line3 = vendor is null ? "" : vendor.Address.GetLine4(),
                Line4 = vendor?.Phone ?? "",
            },
            Customer = new() {
                Name = customer?.Name ?? "",
                Line1 = customer?.BillingAddress.Line1 ?? "",
                Line2 = customer?.BillingAddress.Line2 ?? "",
                Line3 = customer is null ? "" : customer.BillingAddress.GetLine4(),
                Line4 = customer?.BillingContact.Phone ?? "",
            },
            Cabinets = order.Products
                            .OfType<Cabinet>()
                            .Select(cab => new CabinetItem() {
                                Line = cab.ProductNumber,
                                Qty = cab.Qty,
                                Height = cab.Height,
                                Width = cab.Width,
                                Depth = cab.Depth,
                                Description = cab.GetDescription(),
                                UnitPrice = cab.UnitPrice,
                            }).ToList(),
            Doors = order.Products
                            .OfType<MDFDoorProduct>()
                            .Select(cab => new DoorItem() {
                                Line = cab.ProductNumber,
                                Qty = cab.Qty,
                                Height = cab.Height,
                                Width = cab.Width,
                                Description = cab.GetDescription(),
                                UnitPrice = cab.UnitPrice,
                            }).ToList(),
            ClosetParts = order.Products
                            .OfType<IClosetPartProduct>()
                            .Select(cab => new ClosetPartItem() {
                                Line = cab.ProductNumber,
                                Qty = cab.Qty,
                                Length = cab.Length,
                                Width = cab.Width,
                                Description = cab.GetDescription(),
                                UnitPrice = cab.UnitPrice,
                            }).ToList(),
            DrawerBoxes = order.Products
                            .OfType<DovetailDrawerBoxProduct>()
                            .Select(cab => new DrawerBoxItem() {
                                Line = cab.ProductNumber,
                                Qty = cab.Qty,
                                Height = cab.Height,
                                Width = cab.Width,
                                Depth = cab.Depth,
                                Description = cab.GetDescription(),
                                UnitPrice = cab.UnitPrice,
                            }).ToList()
        };

    }

}