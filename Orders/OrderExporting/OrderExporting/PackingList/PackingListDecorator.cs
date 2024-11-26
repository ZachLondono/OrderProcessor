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

namespace OrderExporting.PackingList;

public class PackingListDecorator(PackingList packingList) : IDocumentDecorator {

    private readonly PackingList _packingList = packingList;

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

                    if (_packingList.CounterTops.Any()) {
                        ComposeCounterTopTable(column.Item(), _packingList.CounterTops);
                    }

                    if (_packingList.AdditionalItems.Any()) {
                        ComposeAdditionalItemTable(column.Item(), _packingList.AdditionalItems);
                    }

                    if (_packingList.IncludeSignatureField) {
                        byte[] bytes = Convert.FromBase64String(@"iVBORw0KGgoAAAANSUhEUgAAAN8AAABpCAMAAACNtAZ2AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAALiUExURf7+/v////39/cnJyYCAgHl5eXh4eHd3d4GBgc3NzZSUlBsbG42NjZeXlxkZGZOTkz09Pfr6+j4+PpiYmDk5Oe/v7/z8/Pv7+zs7O/Dw8Do6OuDg4MbGxsfHx8TExMXFxc7OztjY2Pn5+cPDw+np6fPz89HR0cLCwvX19ePj4+jo6Pf399XV1e7u7s/Pz8jIyOfn5xwcHBoaGhcXFxYWFhUVFTQ0NLi4uNLS0hgYGGZmZu3t7XR0dDMzM5qamlNTU0JCQuHh4bW1tSYmJmpqalFRUdTU1ERERElJSczMzCsrKx4eHrm5uYeHhwYGBmhoaKKioqOjo4mJiSEhIfHx8SIiIi8vL5+fn56enp2dnZycnLy8vF5eXgICAkhISKGhoYuLiwMDA+vr6woKCq+vr6qqqgAAAIWFhR8fHygoKNvb27KysgQEBGJiYt7e3icnJ6CgoAgICHx8fAsLCyUlJYiIiKurq3FxcSQkJFBQUGBgYLCwsENDQzw8PBEREampqYSEhH5+fgEBAdbW1t3d3SkpKV9fX46Ojtzc3MvLy0VFRQUFBdPT03p6ent7e76+vgkJCaampvb29jc3Ny0tLezs7CoqKnNzc4qKigcHB5aWlg8PD7Gxsfj4+EFBQRISEmtray4uLtDQ0H9/f11dXeXl5TU1NU5OTn19fSMjI0pKSvLy8uTk5Orq6kBAQDY2Njg4OA0NDbS0tG9vbxAQEIKCgj8/P2FhYaWlpb+/v729vcHBwTExMaysrObm5kdHR8rKyktLS8DAwIODgxQUFJKSktnZ2aenp5CQkPT09K6uro+Pj5mZmZWVlW1tbdfX11VVVd/f39ra2mdnZ01NTWxsbGlpaUZGRmNjY2RkZFhYWIaGhlRUVK2trVZWVqioqFtbW09PT7e3t1xcXG5ubmVlZVdXV7u7uywsLLOzs1JSUqSkpHZ2drq6ujAwMFpaWllZWXJycnV1dba2tiAgIAAAAFeVhuAAAAD2dFJOU///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////AEo/IKkAAAAJcEhZcwAADsQAAA7EAZUrDhsAAAWOSURBVHhe7VtLkuMoEG1WOk1egAjWRB6AI6AbsNWK47HgFlxjMvno40JV3TMT0SD7CZMfcIWfH4nKsvxLPBu/BC6PhUPiFzYXngm3BeanqpTPgyr8oIaPQ+VXo8cBHq5f41fDx+Hp+j19f/nU39x4F/1u+HXSv/VODPN2dfnV4JwjH/JxBkd54MDrlL+Ob+vv37/WUVj216dPCVPyNIxkKeAX7DHRLEAeSgiQPDeaTXlKcRNABumJo9C74ec2hkoCVfYkTZSGMkH4mDPa2y14HhBeK1+maYE0iWwqf2YA9PmFVVpLJMEbY22w9Hrjqm3YVgS9BspYYVcnjCGN0xrBmJLETdET143SY6Dxq2HFslnuV48mlkxaWaqkLKitZJhfWGkeTfNGlyTmaYGGBsHN+lyd96j5hRv+kJ+IRChDpB9nWL9F+FUCKAO0PksSV34/droD4IafIZAKaFYuqCDkxjVFs7xeucZk1o+5IfMs02h+1s+rYf4futNPu40XGSuRT2kLr0QBHkg/SgBk/WglYlgTE6I5lCz6gXn5e38P/fpzVH+0FSJ1l/oLZrnUH6Ud7Z7Hgiz6Zepj4NDv/I47ViutivZPxee/xLK5ZNVqQW8lk/lRxIxo/yxJXDXyNDphjoEb/fJqlKur9aeQ625baZMBlTMGMz9atzy11p9C3MhZ8+47Bvr1h4EFABu8LZcRediHwP/RpHKp1PuQd5xgufJsoKwLkJ1x2B38ashoVK+UX0PGzcyR0NfvjzAwO8J/5zc0/gf9hkZ//3wO3kW/D7858T7191AF30a/Z+LDb268z/75THz4TY5P/U2ND7+50fjV8HE49LsoOL2cjUBHv9m50es/KDy9/jrXPwH4a7DcyvHqtG5v5+7ktG5vP0a76bZOVLtzlFGpdPQDsSilL0cxLWpxc5utxymq5nePq3k9rm6zvYMg9+2kU3/1K5WZwd/5FNzV32s8Kzr6EbyUixf8ddgVUG8ymAd9/fSCTgu+DwnBC+/5NiWPfH8IUu1yCpDG6uyR0eenrBAoluRpv1iA9gwJjlxYNueCSFIo7ZLWVMajo8/PEyMUMUUnogSFXqFFNIhROCdSFCbRe4BxguXaqz9egM6AtNoLK0EDUbUxZn5LEDYKBWBi1G74fejQb3+l5GoqLW2ljUkslZ+ixZr1WzI/T/r5NM59dHfo759SLVEKmZKSSnripzFKbWidhmSkpvoDeo4keefR7wLPN0Ekb9E6KRJQA5sQBSaBpJvgE6j/ev4YDz1+1ScjtYv1LJCT1LWJzQ6OG/0abL4Rpos5CPbrr2ESkb7BD/rNj49+U+PDb258v3/Oj/dbn49gupPo6Zc/S3DiYs7tO9Nt35lu+w3Tbc2UjvCl/trAM3Dot/Oyy+JyI+Seu9Mj54rb+he3OaWVsJnc14HSl6Cm2oNRbTHXJPf0KO0cli7fL57xZf/k67v1Smk1r9dR8wA9crcfJ5eP12ftprmvzzi5dBzzD9Msm2LZ7Mmzf1wYer/9k+GkXNq1B/pUOzH6/JQLzhBB/qJC0if5edX9Un8ZypOGEYKUKRkJVi6zinijH/HzGhd00UeHMYX6M6v5sOt3Zkj8wCsRtIp+SdZEXX80Nh3u9QvSarSS+emEI/2m6E/Q52dcCAqTDlED1Z4Ojn/WNyP6/GhjofMD7S8hAC6QFjn+leo++vvnc9DX7zl4H/0equCn/qbGh9/ceJ/986F4G/2eqeCb1J+xKdlkSzu6m2hvxZ78kvjSanc15/Y1zt3h7+3n6DB8GOZnN2WMooPb0eVMJ8rB4R1+Tby22h0+W35edo+xve325O+ZS3iNsqkdx9xZ4se3CO6AvWNcI8YRnNPst8dpJDu1O/zThIw6tmfPk87+PqHh+zhHQvwDaKgSrAoiK4YAAAAASUVORK5CYII=");
                        column.Item()
                            .PaddingTop(50)
                            .AlignCenter()
                            .Width(250)
                            .Image(bytes);
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

    public void ComposeCabinetTable(IContainer container, IEnumerable<CabinetItem> items) {
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
                        if (_packingList.IncludeCheckBoxesNextToItems) {
                            column.ConstantColumn(20);
                            column.ConstantColumn(30);
                            column.ConstantColumn(30);
                        } else {
                            column.ConstantColumn(40);
                            column.ConstantColumn(40);
                        }
                        column.RelativeColumn();
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                    });

                    table.Header(header => {


                        if (_packingList.IncludeCheckBoxesNextToItems) header.Cell();
                        header.Cell().ColumnSpan(3).PaddingLeft(10).Text($"Cabinets ({items.Sum(i => i.Qty)})").FontSize(16).Bold().Italic();
                        header.Cell().ColumnSpan(3).Element(headerCellStyle).Text("Inches");
                        header.Cell().ColumnSpan(3).Element(headerCellStyle).Text("Millimeters");

                        if (_packingList.IncludeCheckBoxesNextToItems) header.Cell().Element(headerCellStyle).Text("x");
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

                    foreach (var item in items.OrderBy(i => i.Line)) {
                        if (_packingList.IncludeCheckBoxesNextToItems) table.Cell().Element(defaultCellStyle);
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

    public void ComposeCabinetPartTable(IContainer container, IEnumerable<CabinetPartItem> items) {
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
                        if (_packingList.IncludeCheckBoxesNextToItems) column.ConstantColumn(20);
                        column.ConstantColumn(40);
                        column.ConstantColumn(40);
                        column.RelativeColumn();
                    });


                    table.Header(header => {

                        if (_packingList.IncludeCheckBoxesNextToItems) header.Cell();
                        header.Cell().ColumnSpan(3).PaddingLeft(10).Text($"Cabinet Extras ({items.Sum(i => i.Qty)})").FontSize(16).Bold().Italic();

                        if (_packingList.IncludeCheckBoxesNextToItems) header.Cell().Element(headerCellStyle).Text("x");
                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Description");

                    });

                    foreach (var item in items.OrderBy(i => i.Line)) {
                        if (_packingList.IncludeCheckBoxesNextToItems) table.Cell().Element(defaultCellStyle);
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Description.ToString());
                    }

                });

        });
    }

    public void ComposeClosetPartTable(IContainer container, IEnumerable<ClosetPartItem> items) {
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
                        if (_packingList.IncludeCheckBoxesNextToItems) {
                            column.ConstantColumn(20);
                            column.ConstantColumn(30);
                            column.ConstantColumn(30);
                        } else {
                            column.ConstantColumn(40);
                            column.ConstantColumn(40);
                        }
                        column.RelativeColumn();
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                    });


                    table.Header(header => {

                        if (_packingList.IncludeCheckBoxesNextToItems) header.Cell();
                        header.Cell().ColumnSpan(3).PaddingLeft(10).Text($"Closet Parts ({items.Sum(i => i.Qty)})").FontSize(16).Bold().Italic();
                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Inches");
                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Millimeters");

                        if (_packingList.IncludeCheckBoxesNextToItems) header.Cell().Element(headerCellStyle).Text("x");
                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Description");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Length");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Length");

                    });

                    foreach (var item in items.OrderBy(i => i.Line)) {
                        if (_packingList.IncludeCheckBoxesNextToItems) table.Cell().Element(defaultCellStyle);
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

    public void ComposeZargenDrawerTable(IContainer container, IEnumerable<ZargenDrawerItem> items) {
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
                        if (_packingList.IncludeCheckBoxesNextToItems) {
                            column.ConstantColumn(20);
                            column.ConstantColumn(30);
                            column.ConstantColumn(30);
                        } else {
                            column.ConstantColumn(40);
                            column.ConstantColumn(40);
                        }
                        column.RelativeColumn();
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                    });


                    table.Header(header => {

                        if (_packingList.IncludeCheckBoxesNextToItems) header.Cell();
                        header.Cell().ColumnSpan(3).PaddingLeft(10).Text($"Zargen Drawers ({items.Sum(i => i.Qty)})").FontSize(16).Bold().Italic();
                        header.Cell().ColumnSpan(3).Element(headerCellStyle).Text("Inches");
                        header.Cell().ColumnSpan(3).Element(headerCellStyle).Text("Millimeters");

                        if (_packingList.IncludeCheckBoxesNextToItems) header.Cell().Element(headerCellStyle).Text("x");
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

                    foreach (var item in items.OrderBy(i => i.Line)) {
                        if (_packingList.IncludeCheckBoxesNextToItems) table.Cell().Element(defaultCellStyle);
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

    public void ComposeMDFDoorTable(IContainer container, IEnumerable<MDFDoorItem> items) {

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
                        if (_packingList.IncludeCheckBoxesNextToItems) {
                            column.ConstantColumn(20);
                            column.ConstantColumn(30);
                            column.ConstantColumn(30);
                        } else {
                            column.ConstantColumn(40);
                            column.ConstantColumn(40);
                        }
                        column.RelativeColumn();
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                    });


                    table.Header(header => {

                        if (_packingList.IncludeCheckBoxesNextToItems) header.Cell();
                        header.Cell().ColumnSpan(3).PaddingLeft(10).Text($"MDF Doors ({items.Sum(i => i.Qty)})").FontSize(16).Bold().Italic();
                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Inches");
                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Millimeters");

                        if (_packingList.IncludeCheckBoxesNextToItems) header.Cell().Element(headerCellStyle).Text("x");
                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Description");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Height");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Height");

                    });

                    foreach (var item in items.OrderBy(i => i.Line)) {
                        if (_packingList.IncludeCheckBoxesNextToItems) table.Cell().Element(defaultCellStyle);
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

    public void ComposeFivePieceDoorTable(IContainer container, IEnumerable<FivePieceDoorItem> items) {

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
                        if (_packingList.IncludeCheckBoxesNextToItems) {
                            column.ConstantColumn(20);
                            column.ConstantColumn(30);
                            column.ConstantColumn(30);
                        } else {
                            column.ConstantColumn(40);
                            column.ConstantColumn(40);
                        }
                        column.RelativeColumn();
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                    });


                    table.Header(header => {

                        if (_packingList.IncludeCheckBoxesNextToItems) header.Cell();
                        header.Cell().ColumnSpan(3).PaddingLeft(10).Text($"Five-Piece Doors ({items.Sum(i => i.Qty)})").FontSize(16).Bold().Italic();
                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Inches");
                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Millimeters");

                        if (_packingList.IncludeCheckBoxesNextToItems) header.Cell().Element(headerCellStyle).Text("x");
                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Description");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Height");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Height");

                    });

                    foreach (var item in items.OrderBy(i => i.Line)) {
                        if (_packingList.IncludeCheckBoxesNextToItems) table.Cell().Element(defaultCellStyle);
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

    private void ComposeDoweledDrawerBoxTable(IContainer container, IEnumerable<DoweledDrawerBoxItem> items) {

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
                        if (_packingList.IncludeCheckBoxesNextToItems) {
                            column.ConstantColumn(20);
                            column.ConstantColumn(30);
                            column.ConstantColumn(30);
                        } else {
                            column.ConstantColumn(40);
                            column.ConstantColumn(40);
                        }
                        column.ConstantColumn(200);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                    });


                    table.Header(header => {

                        if (_packingList.IncludeCheckBoxesNextToItems) header.Cell();
                        header.Cell().ColumnSpan(3).PaddingLeft(10).Text($"Doweled Drawer Boxes ({items.Sum(i => i.Qty)})").FontSize(16).Bold().Italic();
                        header.Cell().ColumnSpan(3).Element(headerCellStyle).Text("Inches");
                        header.Cell().ColumnSpan(3).Element(headerCellStyle).Text("Millimeters");

                        if (_packingList.IncludeCheckBoxesNextToItems) header.Cell().Element(headerCellStyle).Text("x");
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


                    foreach (var item in items.OrderBy(i => i.Line)) {

                        if (_packingList.IncludeCheckBoxesNextToItems) table.Cell().Element(defaultCellStyle);
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

    public void ComposeDovetailDrawerBoxTable(IContainer container, IEnumerable<DovetailDovetailDrawerBoxItem> items) {

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
                        if (_packingList.IncludeCheckBoxesNextToItems) {
                            column.ConstantColumn(20);
                            column.ConstantColumn(30);
                            column.ConstantColumn(30);
                        } else {
                            column.ConstantColumn(40);
                            column.ConstantColumn(40);
                        }
                        column.RelativeColumn();
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                        column.ConstantColumn(45);
                    });


                    table.Header(header => {

                        if (_packingList.IncludeCheckBoxesNextToItems) header.Cell();
                        header.Cell().ColumnSpan(3).PaddingLeft(10).Text($"Dovetail Drawer Boxes ({items.Sum(i => i.Qty)})").FontSize(16).Bold().Italic();
                        header.Cell().ColumnSpan(3).Element(headerCellStyle).Text("Inches");
                        header.Cell().ColumnSpan(3).Element(headerCellStyle).Text("Millimeters");

                        if (_packingList.IncludeCheckBoxesNextToItems) header.Cell().Element(headerCellStyle).Text("x");
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

                    foreach (var item in items.OrderBy(i => i.Line)) {
                        if (_packingList.IncludeCheckBoxesNextToItems) table.Cell().Element(defaultCellStyle);
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

    public void ComposeCounterTopTable(IContainer container, IEnumerable<CounterTopItem> items) {

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
                .Text($"Counter Tops ({items.Sum(i => i.Qty)})")
                .FontSize(16)
                .Bold()
                .Italic();

            col.Item()
                .DefaultTextStyle(x => x.FontSize(10))
                .Table(table => {

                    table.ColumnsDefinition(column => {
                        if (_packingList.IncludeCheckBoxesNextToItems) column.ConstantColumn(20);
                        column.ConstantColumn(40);
                        column.ConstantColumn(40);
                        column.RelativeColumn();
                        column.ConstantColumn(40);
                        column.ConstantColumn(40);
                        column.ConstantColumn(40);
                    });

                    table.Header(header => {

                        if (_packingList.IncludeCheckBoxesNextToItems) header.Cell().Element(headerCellStyle).Text("x");
                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Finish");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Length");
                        header.Cell().Element(headerCellStyle).Text("Edges");

                    });

                    foreach (var item in items.OrderBy(i => i.Line)) {
                        if (_packingList.IncludeCheckBoxesNextToItems) table.Cell().Element(defaultCellStyle);
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Finish);
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Width.AsMillimeters().ToString("0"));
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.Length.AsMillimeters().ToString("0"));
                        table.Cell().Element(defaultCellStyle).AlignLeft().PaddingLeft(5).Text(item.FinishedEdges);
                    }

                });

        });

    }

    public void ComposeAdditionalItemTable(IContainer container, IEnumerable<AdditionalItem> items) {

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
                        if (_packingList.IncludeCheckBoxesNextToItems) column.ConstantColumn(20);
                        column.ConstantColumn(40);
                        column.ConstantColumn(40);
                        column.RelativeColumn();
                    });

                    table.Header(header => {

                        if (_packingList.IncludeCheckBoxesNextToItems) header.Cell().Element(headerCellStyle).Text("x");
                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Description");

                    });

                    foreach (var item in items.OrderBy(i => i.Line)) {
                        if (_packingList.IncludeCheckBoxesNextToItems) table.Cell().Element(defaultCellStyle);
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
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

}

