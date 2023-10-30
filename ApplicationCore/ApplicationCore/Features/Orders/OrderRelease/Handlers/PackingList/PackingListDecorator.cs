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

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.PackingList;

internal class PackingListDecorator : IPackingListDecorator {

    private PackingList? _packingList = null;

    private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerByIdAsync;
    private readonly CompanyDirectory.GetVendorByIdAsync _getVendorByIdAsync;

    public PackingListDecorator(CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync, CompanyDirectory.GetVendorByIdAsync getVendorByIdAsync) {
        _getCustomerByIdAsync = getCustomerByIdAsync;
        _getVendorByIdAsync = getVendorByIdAsync;
    }

    public async Task AddData(Order order) {
        _packingList = await CreatePackingListModel(order);
    }

    public void Decorate(IDocumentContainer container) {

        if (_packingList is null) {
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
                            .Text("Packing List")
                            .FontSize(36)
                            .Bold();

                    ComposeHeader(column.Item(), _packingList);

                    if (_packingList.Cabinets.Any()) {
                        ComposeCabinetTable(column.Item(), _packingList.Cabinets);
                    }

                    if (_packingList.CabinetParts.Any()) {
                        ComposeCabinetPartTable(column.Item(), _packingList.CabinetParts);
                    }

                    if (_packingList.ClosetParts.Any()) {
                        ComposeClosetPartTable(column.Item(), _packingList.ClosetParts);
                    }

                    if (_packingList.ZargenDrawers.Any()) {
                        ComposeZargenDrawerTable(column.Item(), _packingList.ZargenDrawers);
                    }

                    if (_packingList.MDFDoors.Any()) {
                        ComposeMDFDoorTable(column.Item(), _packingList.MDFDoors);
                    }

                    if (_packingList.FivePieceDoors.Any()) {
                        ComposeFivePieceDoorTable(column.Item(), _packingList.FivePieceDoors);
                    }

                    if (_packingList.DovetailDrawerBoxes.Any()) {
                        ComposeDovetailDrawerBoxTable(column.Item(), _packingList.DovetailDrawerBoxes);
                    }

                    if (_packingList.DoweledDrawerBoxes.Any()) {
                        ComposeDoweledDrawerBoxTable(column.Item(), _packingList.DoweledDrawerBoxes);
                    }

                    if (_packingList.AdditionalItems.Any()) {
                        ComposeAdditionalItemTable(column.Item(), _packingList.AdditionalItems);
                    }

                });

        });

    }

    private static void ComposeHeader(IContainer container, PackingList packingList) {

        container.PaddingBottom(10).Row(row => {

            row.RelativeItem()
                .Column(companyCol => {

                    companyCol.Item()
                              .Element(e => ComposeCompanyInfo(e, "From:", packingList.Vendor));

                    companyCol.Item()
                            .PaddingTop(15)
                            .Element(e => ComposeCompanyInfo(e, "To:", packingList.Customer));

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
                                        .Text(packingList.Date.ToString("MMMM d, yyyy"))
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
                                                .Text(packingList.OrderNumber)
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
                                        .Text(packingList.OrderName)
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

    public static void ComposeCabinetTable(IContainer container, IEnumerable<CabinetItem> items) {
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

                        header.Cell().ColumnSpan(3).PaddingLeft(10).Text($"Cabinets ({items.Sum(i => i.Qty)})").FontSize(16).Bold().Italic();
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

                    foreach (var item in items) {
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Width, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Height, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Depth, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Width.AsMillimeters().ToString("0"));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Height.AsMillimeters().ToString("0"));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Depth.AsMillimeters().ToString("0"));
                    }

                });

        });
    }

    public static void ComposeCabinetPartTable(IContainer container, IEnumerable<CabinetPartItem> items) {
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
                .PaddingTop(20)
                .DefaultTextStyle(x => x.FontSize(10))
                .Table(table => {

                    table.ColumnsDefinition(column => {
                        column.ConstantColumn(40);
                        column.ConstantColumn(40);
                        column.RelativeColumn();
                    });


                    table.Header(header => {

                        header.Cell().ColumnSpan(3).PaddingLeft(10).Text($"Cabinet Extras ({items.Sum(i => i.Qty)})").FontSize(16).Bold().Italic();

                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Description");

                    });

                    foreach (var item in items) {
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description.ToString());
                    }

                });

        });
    }


    public static void ComposeClosetPartTable(IContainer container, IEnumerable<ClosetPartItem> items) {
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
                    });


                    table.Header(header => {

                        header.Cell().ColumnSpan(3).PaddingLeft(10).Text($"Closet Parts ({items.Sum(i => i.Qty)})").FontSize(16).Bold().Italic();
                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Inches");
                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Millimeters");

                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Description");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Length");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Length");

                    });

                    foreach (var item in items) {
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Width, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Length, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Width.AsMillimeters().ToString("0"));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Length.AsMillimeters().ToString("0"));
                    }

                });

        });
    }

    public static void ComposeZargenDrawerTable(IContainer container, IEnumerable<ZargenDrawerItem> items) {
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

                        header.Cell().ColumnSpan(3).PaddingLeft(10).Text($"Zargen Drawers ({items.Sum(i => i.Qty)})").FontSize(16).Bold().Italic();
                        header.Cell().ColumnSpan(3).Element(headerCellStyle).Text("Inches");
                        header.Cell().ColumnSpan(3).Element(headerCellStyle).Text("Millimeters");

                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Description");
                        header.Cell().Element(headerCellStyle).Text("OpeningWidth");
                        header.Cell().Element(headerCellStyle).Text("Height");
                        header.Cell().Element(headerCellStyle).Text("Depth");
                        header.Cell().Element(headerCellStyle).Text("OpeningWidth");
                        header.Cell().Element(headerCellStyle).Text("Height");
                        header.Cell().Element(headerCellStyle).Text("Depth");

                    });

                    foreach (var item in items) {
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.OpeningWidth, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Height, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Depth, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.OpeningWidth.AsMillimeters().ToString("0"));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Height.AsMillimeters().ToString("0"));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Depth.AsMillimeters().ToString("0"));
                    }

                });

        });
    }

    public static void ComposeMDFDoorTable(IContainer container, IEnumerable<MDFDoorItem> items) {

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
                    });


                    table.Header(header => {

                        header.Cell().ColumnSpan(3).PaddingLeft(10).Text($"MDF Doors ({items.Sum(i => i.Qty)})").FontSize(16).Bold().Italic();
                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Inches");
                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Millimeters");

                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Description");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Height");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Height");

                    });

                    foreach (var item in items) {
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Width, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Height, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Width.AsMillimeters().ToString("0"));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Height.AsMillimeters().ToString("0"));
                    }

                });

        });

    }

    public static void ComposeFivePieceDoorTable(IContainer container, IEnumerable<FivePieceDoorItem> items) {

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
                    });


                    table.Header(header => {

                        header.Cell().ColumnSpan(3).PaddingLeft(10).Text($"Five-Piece Doors ({items.Sum(i => i.Qty)})").FontSize(16).Bold().Italic();
                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Inches");
                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Millimeters");

                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Description");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Height");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Height");

                    });

                    foreach (var item in items) {
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Width, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Height, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Width.AsMillimeters().ToString("0"));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Height.AsMillimeters().ToString("0"));
                    }

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
                .PaddingTop(20)
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
                        column.ConstantColumn(45);
                    });


                    table.Header(header => {

                        header.Cell().ColumnSpan(3).PaddingLeft(10).Text($"Doweled Drawer Boxes ({items.Sum(i => i.Qty)})").FontSize(16).Bold().Italic();
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


                    foreach (var item in items) {

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

        });

    }

    public static void ComposeDovetailDrawerBoxTable(IContainer container, IEnumerable<DovetailDovetailDrawerBoxItem> items) {

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

                        header.Cell().ColumnSpan(3).PaddingLeft(10).Text($"Dovetail Drawer Boxes ({items.Sum(i => i.Qty)})").FontSize(16).Bold().Italic();
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

                    foreach (var item in items) {
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Width, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Height, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Depth, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Width.AsMillimeters().ToString("0"));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Height.AsMillimeters().ToString("0"));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Depth.AsMillimeters().ToString("0"));
                    }

                });

        });

    }

    public static void ComposeAdditionalItemTable(IContainer container, IEnumerable<AdditionalItem> items) {

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
                .Text($"Additional Items ({items.Count()})")
                .FontSize(16)
                .Bold()
                .Italic();

            col.Item()
                .DefaultTextStyle(x => x.FontSize(10))
                .Table(table => {

                    table.ColumnsDefinition(column => {
                        column.ConstantColumn(40);
                        column.RelativeColumn();
                    });

                    table.Header(header => {

                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Description");

                    });

                    foreach (var item in items) {
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description.ToString());
                    }

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

    private async Task<PackingList> CreatePackingListModel(Order order) {

        var vendor = await _getVendorByIdAsync(order.VendorId);
        var customer = await _getCustomerByIdAsync(order.CustomerId);

        return new PackingList() {
            OrderNumber = order.Number,
            OrderName = order.Name,
            Date = DateTime.Today,
            Vendor = new() {
                Name = vendor?.Name ?? "",
                Line1 = vendor?.Address.Line1 ?? "",
                Line2 = vendor?.Address.Line2 ?? "",
                Line3 = vendor is null ? "" : vendor.Address.GetLine4(),
                Line4 = vendor?.Phone ?? "",
            },
            Customer = new() {
                Name = customer?.Name ?? "",
                Line1 = order.Shipping.Address.Line1,
                Line2 = order.Shipping.Address.Line2,
                Line3 = order.Shipping.Address.GetLine4(),
                Line4 = order.Shipping.PhoneNumber,
            },
            Cabinets = order.Products
                            .OfType<Cabinet>()
                            .Select(cab => new CabinetItem() {
                                Line = cab.ProductNumber,
                                Qty = cab.Qty,
                                Height = cab.Height,
                                Width = cab.Width,
                                Depth = cab.Depth,
                                Description = cab.GetDescription()
                            }).ToList(),
            CabinetParts = order.Products
                                .OfType<CabinetPart>()
                                .Select(cabPart => new CabinetPartItem() {
                                    Line = cabPart.ProductNumber,
                                    Qty = cabPart.Qty,
                                    Description = cabPart.GetDescription()
                                }).ToList(),
            MDFDoors = order.Products
                            .OfType<MDFDoorProduct>()
                            .Select(cab => new MDFDoorItem() {
                                Line = cab.ProductNumber,
                                Qty = cab.Qty,
                                Height = cab.Height,
                                Width = cab.Width,
                                Description = cab.GetDescription()
                            }).ToList(),
            FivePieceDoors = order.Products
                            .OfType<FivePieceDoorProduct>()
                            .Select(cab => new FivePieceDoorItem() {
                                Line = cab.ProductNumber,
                                Qty = cab.Qty,
                                Height = cab.Height,
                                Width = cab.Width,
                                Description = cab.GetDescription()
                            }).ToList(),
            ClosetParts = order.Products
                            .OfType<IClosetPartProduct>()
                            .Select(cab => new ClosetPartItem() {
                                Line = cab.ProductNumber,
                                Qty = cab.Qty,
                                Length = cab.Length,
                                Width = cab.Width,
                                Description = cab.GetDescription()
                            }).ToList(),
            ZargenDrawers = order.Products
                            .OfType<ZargenDrawer>()
                            .Select(cab => new ZargenDrawerItem() {
                                Line = cab.ProductNumber,
                                Qty = cab.Qty,
                                Height = cab.Height,
                                Depth = cab.Depth,
                                OpeningWidth = cab.OpeningWidth,
                                Description = cab.GetDescription()
                            }).ToList(),
            DovetailDrawerBoxes = order.Products
                            .OfType<DovetailDrawerBoxProduct>()
                            .Select(cab => new DovetailDovetailDrawerBoxItem() {
                                Line = cab.ProductNumber,
                                Qty = cab.Qty,
                                Height = cab.Height,
                                Width = cab.Width,
                                Depth = cab.Depth,
                                Description = cab.GetDescription()
                            }).ToList(),
            DoweledDrawerBoxes = order.Products
                            .OfType<DoweledDrawerBoxProduct>()
                            .Select(cab => new DoweledDrawerBoxItem() {
                                Line = cab.ProductNumber,
                                Qty = cab.Qty,
                                Height = cab.Height,
                                Width = cab.Width,
                                Depth = cab.Depth,
                                Description = cab.GetDescription()
                            }).ToList(),
            AdditionalItems = order.AdditionalItems
                            .Where(i => !i.IsService)
                            .Select((item, idx) => new AdditionalItem() {
                                Line = idx + 1,
                                Description = item.Description
                            }).ToList()
        };

    }

}

