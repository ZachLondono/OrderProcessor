using Domain.Orders.Entities;
using Domain.Companies.Entities;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.Enums;
using Domain.Orders.Entities.Products.Closets;
using static OrderExporting.JobSummary.ClosetPartGroup;
using static OrderExporting.JobSummary.DoweledDrawerBoxGroup;
using static OrderExporting.JobSummary.DovetailDrawerBoxGroup;
using static OrderExporting.JobSummary.ZargenDrawerGroup;
using static OrderExporting.JobSummary.CabinetGroup;
using static OrderExporting.JobSummary.MDFDoorGroup;
using static OrderExporting.JobSummary.FivePieceDoorGroup;
using static OrderExporting.JobSummary.CabinetPartGroup;
using Domain.Orders.Entities.Products.DrawerBoxes;
using Domain.Orders.Entities.Products.Doors;
using Domain.Orders;

namespace OrderExporting.JobSummary;

public class JobSummaryModelFactory {

    public static JobSummary CreateSummary(Order order, Vendor vendor, Customer customer, bool showItems, bool showMaterialTypes, string[] materialTypes) {

        var dovetailDb = order.Products
                    .OfType<DovetailDrawerBoxProduct>()
                    .GroupBy(b => new DovetailDrawerBoxGroup() {
                        Room = b.Room,
                        Material = b.DrawerBoxOptions.GetMaterialFriendlyName(),
                        BottomMaterial = b.DrawerBoxOptions.BottomMaterial,
                        Clips = b.DrawerBoxOptions.Clips,
                        Notch = b.DrawerBoxOptions.Notches
                    }, new DrawerBoxGroupComparer())
                    .Select(g => {
                        g.Key.Items = g.Select(i => new DovetailDrawerBoxItem() {
                            Line = i.ProductNumber,
                            Qty = i.Qty,
                            Description = i.GetDescription(),
                            Logo = i.DrawerBoxOptions.Logo != LogoPosition.None,
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

        var doweledDb = order.Products
                            .OfType<DoweledDrawerBoxProduct>()
                            .GroupBy(b => new DoweledDrawerBoxGroup() {
                                Room = b.Room,
                                FrontMaterial = b.FrontMaterial,
                                BackMaterial = b.BackMaterial,
                                SideMaterial = b.SideMaterial,
                                BottomMaterial = b.BottomMaterial,
                                MachineForUMSlides = b.MachineThicknessForUMSlides
                            }, new DoweledDrawerBoxGroupComparer())
                            .Select(g => {
                                g.Key.Items = g.Select(i => new DoweledDrawerBoxItem() {
                                    Line = i.ProductNumber,
                                    Qty = i.Qty,
                                    Description = i.GetDescription(),
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
                    .OfType<IClosetPartProduct>()
                    .GroupBy(p => new ClosetPartGroup() {
                        Room = p.Room,
                        MaterialCore = p.Material.Core.ToString(),
                        MaterialFinish = p.Material.Finish,
                        EdgeBandingFinish = p.EdgeBandingColor,
                        EdgeBandingMaterial = p.Material.Core == ClosetMaterialCore.ParticleBoard ? "PVC" : "Veneer"
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

        var zargens = order.Products
                    .OfType<ZargenDrawer>()
                    .GroupBy(z => new ZargenDrawerGroup() {
                        EdgeBandingFinish = z.EdgeBandingColor,
                        MaterialCore = z.Material.Core.ToString(),
                        MaterialFinish = z.Material.Finish,
                        Room = z.Room
                    }, new ZargenDrawerGroupComparer())
                    .Select(g => {

                        g.Key.Items = g.Select(i => new ZargenDrawerItem() {
                            Line = i.ProductNumber,
                            Qty = i.Qty,
                            Sku = i.SKU,
                            Description = i.GetDescription(),
                            OpeningWidth = i.OpeningWidth,
                            Height = i.Height,
                            Depth = i.Depth
                        })
                        .OrderBy(i => i.Line)
                        .ToList();

                        return g.Key;

                    })
                    .ToList();

        var cabParts = order.Products
                            .OfType<CabinetPart>()
                            .GroupBy(p => new CabinetPartGroup() {
                                Room = p.Room,
                                MaterialCore = p.Material.Core.ToString(),
                                MaterialFinish = p.Material.Finish,
                                EdgeBandingFinish = p.EdgeBandingColor,
                            }, new CabinetPartGroupComparer())
                            .Select(g => {

                                g.Key.Items = g.Select(i => new CabinetPartItem() {
                                    Line = i.ProductNumber,
                                    Qty = i.Qty,
                                    Sku = i.SKU,
                                    Description = i.GetDescription()
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
                            Fronts = p.SlabDoorMaterial is not null ? $"Slab - {p.SlabDoorMaterial.Finish}" : "MDF By Royal",
                            Paint = p.FinishMaterial.PaintColor ?? "",
                            Assembled = p.Assembled
                        }, new CabinetGroupComparer())
                        .Select(g => {

                            g.Key.Items = g.Select(i => {
                                List<string> comments = [];
                                if (!string.IsNullOrEmpty(i.Comment)) comments.Add(i.Comment);

                                return new CabinetItem {
                                    Line = i.ProductNumber,
                                    Qty = i.Qty,
                                    Description = i.GetDescription(),
                                    Height = i.Height,
                                    Width = i.Width,
                                    Depth = i.Depth,
                                    LeftSide = i.LeftSideType,
                                    RightSide = i.RightSideType,
                                    Comments = comments.ToArray()
                                };
                            })
                            .OrderBy(i => i.Line)
                            .ToList();

                            return g.Key;

                        })
                        .ToList();

        var mdfDoors = order.Products
                        .OfType<MDFDoorProduct>()
                        .GroupBy(d => new MDFDoorGroup {
                            Room = d.Room,
                            Finish = d.PaintColor ?? "",
                            Material = d.Material,
                            Style = d.FramingBead
                        }, new MDFDoorGroupComparer())
                        .Select(g => {

                            g.Key.Items = g.Select(i => new MDFDoorItem {
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

        var fivePieceDoors = order.Products
                        .OfType<FivePieceDoorProduct>()
                        .GroupBy(d => new FivePieceDoorGroup {
                            Room = d.Room,
                            Material = d.Material
                        }, new FivePieceDoorGroupComparer())
                        .Select(g => {

                            g.Key.Items = g.Select(i => new FivePieceDoorItem {
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

        bool containsDovetailDBSubComponents = order.Products
                                                    .OfType<IDovetailDrawerBoxContainer>()
                                                    .Any(p => p is not DovetailDrawerBoxProduct && p.ContainsDovetailDrawerBoxes())
                                                    ||
                                                    order.Products
                                                        .OfType<DovetailDrawerBoxProduct>()
                                                        .Any()
                                                    &&
                                                    !order.Products.All(p => p is DovetailDrawerBoxProduct);

        bool containsMDFDoorSubComponents = order.Products
                                                .OfType<IMDFDoorContainer>()
                                                .Any(p => p is not MDFDoorProduct && p.ContainsDoors())
                                                ||
                                                order.Products
                                                    .OfType<MDFDoorProduct>()
                                                    .Any()
                                                &&
                                                !order.Products.All(p => p is MDFDoorProduct);

        bool containsFivePieceDoorSubComponents = order.Products
                                                .OfType<FivePieceDoorProduct>()
                                                .Any()
                                               &&
                                               !order.Products.All(p => p is FivePieceDoorProduct);

        return new JobSummary() {

            Number = order.Number,
            Name = order.Name,
            CustomerName = customer?.Name ?? "",
            VendorLogo = vendor?.Logo ?? Array.Empty<byte>(),
            Comment = order.CustomerComment,
            ReleaseDate = DateTime.Now,
            OrderDate = order.OrderDate,
            DueDate = order.DueDate,

            SpecialRequirements = order.Note,

            ShowItemsInSummary = showItems,
            Cabinets = cabs,
            CabinetParts = cabParts,
            ClosetParts = cp,
            ZargenDrawers = zargens,
            MDFDoors = mdfDoors,
            FivePieceDoors = fivePieceDoors,
            DovetailDrawerBoxes = dovetailDb,
            DoweledDrawerBoxes = doweledDb,
            AdditionalItems = order.AdditionalItems.ToList(),

            ContainsDovetailDBSubComponents = containsDovetailDBSubComponents,
            ContainsMDFDoorSubComponents = containsMDFDoorSubComponents,
            ContainsFivePieceDoorSubComponents = containsFivePieceDoorSubComponents,

            ShowMaterialTypesInSummary = showMaterialTypes,
            MaterialTypes = new(materialTypes),

        };

    }

}
