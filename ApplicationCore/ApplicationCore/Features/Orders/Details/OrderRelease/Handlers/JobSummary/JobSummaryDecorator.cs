using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Shared.Domain;
using static ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.JobSummary.CabinetGroup;
using static ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.JobSummary.ClosetPartGroup;
using static ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.JobSummary.DrawerBoxGroup;
using static ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.JobSummary.DoorGroup;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.JobSummary;

internal class JobSummaryDecorator : IJobSummaryDecorator {

    private readonly CompanyDirectory.GetVendorByIdAsync _getVendorByIdAsync;
    private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerByIdAsync;

    public JobSummaryDecorator(CompanyDirectory.GetVendorByIdAsync getVendorByIdAsync, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync) {
        _getVendorByIdAsync = getVendorByIdAsync;
        _getCustomerByIdAsync = getCustomerByIdAsync;
    }

    public async Task Decorate(Order order, IDocumentContainer container) {

        var jobSummary = await GetJobSummaryModel(order);
        ComposeJobSummary(container, jobSummary);

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
                        .PaddingVertical(30)
                        .Row(row => {

                            row.RelativeItem(1)
                                .AlignCenter()
                                .DefaultTextStyle(style => style.FontSize(12))
                                .Table(table => {

                                    table.ColumnsDefinition(column => {
                                        column.ConstantColumn(90);
                                        column.ConstantColumn(50);
                                    });

                                    int cabQty = jobSummary.Cabients.Sum(p => p.Items.Sum(i => i.Qty));
                                    int cpQty = jobSummary.ClosetParts.Sum(p => p.Items.Sum(i => i.Qty));
                                    int doorQty = jobSummary.Doors.Sum(p => p.Items.Sum(i => i.Qty));
                                    int drawerQty = jobSummary.DrawerBoxes.Sum(p => p.Items.Sum(i => i.Qty));

                                    table.Cell().AlignRight().PaddingRight(5).Text("Cabinets:");
                                    table.Cell().AlignCenter().Text(cabQty == 0 ? "-" : cabQty.ToString());

                                    table.Cell().AlignRight().PaddingRight(5).Text("Closet Parts:");
                                    table.Cell().AlignCenter().Text(cpQty == 0 ? "-" : cpQty.ToString());

                                    table.Cell().AlignRight().PaddingRight(5).Text("MDF Doors:");
                                    table.Cell().AlignCenter().Text(doorQty == 0 ? "-" : doorQty.ToString());

                                    table.Cell().BorderBottom(0.5f).AlignRight().PaddingRight(5).Text("Drawer Boxes:");
                                    table.Cell().BorderBottom(0.5f).AlignCenter().Text(drawerQty == 0 ? "-" : drawerQty.ToString());

                                    table.Cell().AlignRight().PaddingRight(5).Text("Total:").Bold().FontSize(14);
                                    table.Cell().AlignCenter().Text((cabQty + cpQty + doorQty + drawerQty).ToString()).Bold().FontSize(14);

                                });

                            row.RelativeItem(1)
                                .AlignCenter()
                                .DefaultTextStyle(style => style.FontSize(12))
                                .Table(table => {

                                    table.ColumnsDefinition(column => {
                                        column.ConstantColumn(65);
                                        column.ConstantColumn(75);
                                    });

                                    table.Cell().AlignRight().PaddingRight(5).Text("Sub Total:");
                                    table.Cell().AlignCenter().Text($"${jobSummary.SubTotal:0.00}");

                                    table.Cell().AlignRight().PaddingRight(5).Text("Shipping:");
                                    table.Cell().AlignCenter().Text($"${jobSummary.Shipping:0.00}");

                                    table.Cell().BorderBottom(0.5f).AlignRight().PaddingRight(5).Text("Sales Tax:");
                                    table.Cell().BorderBottom(0.5f).AlignCenter().Text($"${jobSummary.SalesTax:0.00}");

                                    table.Cell().AlignRight().PaddingRight(5).Text("Total:").Bold().FontSize(14);
                                    table.Cell().AlignCenter().Text($"${jobSummary.Total:0.00}").Bold().FontSize(14);

                                });

                        });

                    column.Item().PaddingLeft(10).PaddingTop(10).Text("Special Requirements").Italic().Bold().FontSize(16);
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

                    if (jobSummary.ShowItemsInSummary) {
                        foreach (var group in jobSummary.Cabients) {
                            ComposeCabinetTable(column.Item(), group);
                        }

                        foreach (var group in jobSummary.ClosetParts) {
                            ComposeClosetPartTable(column.Item(), group);
                        }

                        foreach (var group in jobSummary.DrawerBoxes) {
                            ComposeDrawerBoxTable(column.Item(), group);
                        }

                        foreach (var group in jobSummary.Doors) {
                            ComposeDoorTable(column.Item(), group);
                        }
                    }

                    if (jobSummary.ShowSuppliesInSummary && jobSummary.Supplies.Any()) {
                        ComposeSuppliesTable(column.Item(), jobSummary.Supplies);
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

    private async Task<JobSummary> GetJobSummaryModel(Order order) {

        var vendor = await _getVendorByIdAsync(order.VendorId);
        var customer = await _getCustomerByIdAsync(order.CustomerId);

        var supplies = order.Products
                            .OfType<Cabinet>()
                            .SelectMany(c => c.GetSupplies())
                            .GroupBy(s => s.Name)
                            .Select(g => new Supply(g.Sum(g => g.Qty), g.Key))
                            .ToList();

        var db = order.Products
                    .OfType<DovetailDrawerBoxProduct>()
                    .GroupBy(b => new DrawerBoxGroup() {
                        Room = b.Room,
                        Material = b.DrawerBoxOptions.GetMaterialFriendlyName(),
                        BottomMaterial = b.DrawerBoxOptions.BottomMaterial,
                        Clips = b.DrawerBoxOptions.Clips,
                        Notch = b.DrawerBoxOptions.Notches
                    }, new DrawerBoxGroupComparer())
                    .Select(g => {
                        g.Key.Items = g.Select(i => new DrawerBoxItem() {
                            Line = i.ProductNumber,
                            Qty = i.Qty,
                            Description = i.GetDescription(),
                            Logo = (i.DrawerBoxOptions.Logo != LogoPosition.None),
                            Scoop = i.DrawerBoxOptions.ScoopFront,
                            Height = i.Height,
                            Width = i.Width,
                            Depth = i.Depth
                        })
                        .OrderBy(i => i.Line)
                        .ToList();
                        return g.Key;
                    })
                    .ToList();

        var cp = order.Products
                    .OfType<ClosetPart>()
                    .GroupBy(p => new ClosetPartGroup() {
                        Room = p.Room,
                        MaterialCore = p.Material.Core.ToString(),
                        MaterialFinish = p.Material.Finish,
                        EdgeBandingFinish = p.EdgeBandingColor,
                        EdgeBandingMaterial = (p.Material.Core == ClosetMaterialCore.ParticleBoard ? "PVC" : "Veneer")
                    }, new ClosetPartGroupComparer())
                    .Select(g => {

                        g.Key.Items = g.Select(i => new ClosetPartItem() {
                            Line = i.ProductNumber,
                            Qty = i.Qty,
                            Sku = i.SKU,
                            Description = i.GetDescription(),
                            Length = i.Length,
                            Width = i.Width
                        })
                        .OrderBy(i => i.Line)
                        .ToList();

                        return g.Key;
                    })
                    .ToList();

        var cabs = order.Products
                        .OfType<Cabinet>()
                        .GroupBy(p => new CabinetGroup() {
                            Room = p.Room,
                            BoxCore = p.BoxMaterial.Core.ToString(),
                            BoxFinish = p.BoxMaterial.Finish,
                            FinishCore = p.FinishMaterial.Core.ToString(),
                            FinishFinish = p.FinishMaterial.Finish,
                            Fronts = p.MDFDoorOptions is  null ? "Slab" : "MDF By Royal",
                            Paint = p.FinishMaterial.PaintColor ?? ""
                        }, new CabinetGroupComparer())
                        .Select(g => {

                            g.Key.Items = g.Select(i => new CabinetItem {
                                Line = i.ProductNumber,
                                Qty = i.Qty,
                                Description = i.GetDescription(),
                                Height = i.Height,
                                Width = i.Width,
                                Depth = i.Depth,
                                FinLeft = i.LeftSideType == CabinetSideType.Finished,
                                FinRight = i.RightSideType == CabinetSideType.Finished
                            })
                            .OrderBy(i => i.Line)
                            .ToList();

                            return g.Key;

                        })
                        .ToList();

        var doors = order.Products
                        .OfType<MDFDoorProduct>()
                        .GroupBy(d => new DoorGroup {
                            Room = d.Room,
                            Finish = d.PaintColor ?? "",
                            Material = d.Material,
                            Style = d.FramingBead
                        }, new DoorGroupComparer())
                        .Select(g => {

                            g.Key.Items = g.Select(i => new DoorItem {
                                Line = i.ProductNumber,
                                Description = i.GetDescription(),
                                Qty = i.Qty,
                                Height = i.Height,
                                Width = i.Width
                            })
                            .OrderBy(i => i.Line)
                            .ToList();

                            return g.Key;

                        })
                        .ToList();

        return new JobSummary() {

            Number = order.Number,
            Name = order.Name,
            CustomerName = customer?.Name ?? "",
            VendorLogo = vendor?.Logo ?? Array.Empty<byte>(),
            Comment = order.CustomerComment,
            ReleaseDate = DateTime.Now,

            SpecialRequirements = order.Note,

            SubTotal = order.SubTotal,
            SalesTax = order.Tax,
            Shipping = order.Shipping.Price,
            Total = order.Total,

            ShowItemsInSummary = false,
            Cabients = cabs,
            ClosetParts = cp,
            Doors = doors,
            DrawerBoxes = db,

            ShowSuppliesInSummary = false,
            Supplies = supplies,

        };

    }

    private static void ComposeCabinetTable(IContainer container, CabinetGroup group) {

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
                        header.Cell().ColumnSpan(1).Element(defaultCellStyle).PaddingLeft(5).Text($"{group.BoxCore} - {group.BoxFinish}");
                        header.Cell().ColumnSpan(1).Element(headerCellStyle).Text("Fronts");
                        header.Cell().ColumnSpan(4).Element(defaultCellStyle).PaddingLeft(5).Text(group.Fronts);

                        header.Cell().ColumnSpan(2).Element(headerCellStyle).Text("Fin Material");
                        header.Cell().ColumnSpan(1).Element(defaultCellStyle).PaddingLeft(5).Text($"{group.FinishCore} - {group.FinishFinish}");
                        header.Cell().ColumnSpan(1).Element(headerCellStyle).Text("Paint");
                        header.Cell().ColumnSpan(4).Element(defaultCellStyle).PaddingLeft(5).Text(group.Paint);

                        header.Cell().Element(headerCellStyle).Text("#");
                        header.Cell().Element(headerCellStyle).Text("Qty");
                        header.Cell().Element(headerCellStyle).Text("Description");
                        header.Cell().Element(headerCellStyle).Text("Width");
                        header.Cell().Element(headerCellStyle).Text("Height");
                        header.Cell().Element(headerCellStyle).Text("Depth");
                        header.Cell().Element(headerCellStyle).Text("Fin L");
                        header.Cell().Element(headerCellStyle).Text("Fin R");

                    });

                    foreach (var item in group.Items) {

                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Line.ToString());
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.Qty.ToString());
                        table.Cell().Element(defaultCellStyle).AlignLeft().Text(item.Description);
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Width, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Height, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(text => FormatFraction(text, item.Depth, 10));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.FinLeft ? "X" : "");
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text(item.FinRight ? "X" : "");

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

    private static void ComposeDoorTable(IContainer container, DoorGroup group) {

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
                .Text($"{(group.Room == "" ? "" : $"{group.Room} - ")}Doors ({group.Items.Sum(i => i.Qty)})")
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

    private static void ComposeDrawerBoxTable(IContainer container, DrawerBoxGroup group) {

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
                .Text($"{(group.Room == "" ? "" : $"{group.Room} - ")}Drawer Boxes ({group.Items.Sum(i => i.Qty)})")
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
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text((item.Logo ? "Y" : ""));
                        table.Cell().Element(defaultCellStyle).AlignCenter().Text((item.Scoop ? "Y" : ""));

                    }

                });

        });

    }

    private static void ComposeSuppliesTable(IContainer container, IEnumerable<Supply> supplies) {

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

        var addRow = (TableDescriptor table, string name, int qty) => {

            table.Cell().Element(defaultCellStyle).Text(name);
            table.Cell().Element(defaultCellStyle).AlignCenter().Text(qty.ToString());

        };

        container.DefaultTextStyle(x => x.FontSize(10))
                .Column(col => {

                    col.Item()
                        .PaddingTop(10)
                        .PaddingLeft(8)
                        .Text("Hardware")
                        .FontSize(14);

                    col.Item()
                        .Table(table => {

                            table.ColumnsDefinition(column => {
                                column.ConstantColumn(150);
                                column.ConstantColumn(50);
                            });


                            table.Header(header => {

                                header.Cell().Element(headerCellStyle).Text("Name");
                                header.Cell().Element(headerCellStyle).Text("Qty");

                            });

                            foreach (var supply in supplies) {
                                addRow(table, supply.Name, supply.Qty);
                            }

                        });

                });

    }

    private static void ComposeHeader(IContainer container, JobSummary jobSummary) {

        container.Column(col => {

            col.Item().Row(row => {

                row.RelativeItem()
                    .PaddingRight(10)
                     .Column(col => {
                         col.Item()
                             .Text($"{jobSummary.Number} {jobSummary.Name}")
                             .SemiBold()
                             .FontSize(20);
                         col.Item()
                             .Text(jobSummary.CustomerName)
                             .FontSize(12);
                         col.Item()
                             .Text(jobSummary.ReleaseDate.ToString("MMMM d, yyyy"))
                             .FontSize(12)
                             .Italic();
                     });

                if (jobSummary.VendorLogo.Any()) {
                    row.ConstantItem(150)
                        .AlignRight()
                        .Image(jobSummary.VendorLogo);
                }

            });

            col.Item().PaddingVertical(5).Text(jobSummary.Comment).FontSize(12);

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
