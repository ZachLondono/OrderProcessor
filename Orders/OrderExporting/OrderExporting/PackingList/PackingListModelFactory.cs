using Domain.Companies.Entities;
using Domain.Orders.Entities;
using Domain.Orders.Entities.Products;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.Entities.Products.Closets;
using Domain.Orders.Entities.Products.Doors;
using Domain.Orders.Entities.Products.DrawerBoxes;
using Domain.Orders.Enums;

namespace OrderExporting.PackingList;

public class PackingListModelFactory {

    public static PackingList CreatePackingListModel(Order order, Vendor vendor, Customer customer, bool checkBoxes, bool signatureField) {

        return new PackingList() {
            IncludeCheckBoxesNextToItems = checkBoxes,
            IncludeSignatureField = signatureField,
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
            CounterTops = order.Products
                            .OfType<CounterTop>()
                            .Select(c => new CounterTopItem() {
                                Line = c.ProductNumber,
                                Qty = c.Qty,
                                Length = c.Length,
                                Width = c.Width,
                                Finish = c.Finish,
                                FinishedEdges = c.EdgeBanding switch {
                                   EdgeBandingSides.None => "None",
                                   EdgeBandingSides.All => "All",
                                   EdgeBandingSides.OneLong => "1L",
                                   EdgeBandingSides.TwoLong => "2L",
                                   EdgeBandingSides.OneShort => "1S",
                                   EdgeBandingSides.TwoShort => "2S",
                                   EdgeBandingSides.TwoLongOneShort => "2L1S",
                                   EdgeBandingSides.OneLongOneShort => "1L1S",
                                   EdgeBandingSides.OneLongTwoShort => "1L2S",
                                   _ => "???"
                                }
                            })
                            .ToList(),
            AdditionalItems = order.AdditionalItems
                            .Select((item, idx) => new AdditionalItem() {
                                Line = idx + 1,
                                Qty = item.Qty,
                                Description = item.Description
                            }).ToList()
        };

    }

}
