using Domain.Companies.Entities;
using Domain.Orders.Entities;
using Domain.Orders.Entities.Products;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.Entities.Products.Closets;
using Domain.Orders.Entities.Products.Doors;
using Domain.Orders.Entities.Products.DrawerBoxes;

namespace OrderExporting.Invoice;

public class InvoiceModelFactory {

    public static Invoice CreateInvoiceModel(Order order, Vendor vendor, string customerName) {

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
                Name = customerName,
                Line1 = order.Billing.Address.Line1,
                Line2 = order.Billing.Address.Line2,
                Line3 = order.Billing.Address.GetLine4(),
                Line4 = order.Billing.PhoneNumber,
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
            CabinetParts = order.Products
                                .OfType<CabinetPart>()
                                .Select(cab => new CabinetPartItem() {
                                    Line = cab.ProductNumber,
                                    Qty = cab.Qty,
                                    Description = cab.GetDescription(),
                                    UnitPrice = cab.UnitPrice
                                }).ToList(),
            MDFDoors = order.Products
                            .OfType<MDFDoorProduct>()
                            .Select(cab => new MDFDoorItem() {
                                Line = cab.ProductNumber,
                                Qty = cab.Qty,
                                Height = cab.Height,
                                Width = cab.Width,
                                Description = cab.GetDescription(),
                                UnitPrice = cab.UnitPrice,
                            }).ToList(),
            FivePieceDoors = order.Products
                            .OfType<FivePieceDoorProduct>()
                            .Select(cab => new FivePieceDoorItem() {
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
            ZargenDrawers = order.Products
                            .OfType<ZargenDrawer>()
                            .Select(cab => new ZargenDrawerItem() {
                                Line = cab.ProductNumber,
                                Qty = cab.Qty,
                                Height = cab.Height,
                                Depth = cab.Depth,
                                OpeningWidth = cab.OpeningWidth,
                                Description = cab.GetDescription(),
                                UnitPrice = cab.UnitPrice,
                            }).ToList(),
            DovetailDrawerBoxes = order.Products
                            .OfType<DovetailDrawerBoxProduct>()
                            .Select(cab => new DovetailDrawerBoxItem() {
                                Line = cab.ProductNumber,
                                Qty = cab.Qty,
                                Height = cab.Height,
                                Width = cab.Width,
                                Depth = cab.Depth,
                                Description = cab.GetDescription(),
                                UnitPrice = cab.UnitPrice,
                            }).ToList(),
            DoweledDrawerBoxes = order.Products
                            .OfType<DoweledDrawerBoxProduct>()
                            .Select(cab => new DoweledDrawerBoxItem() {
                                Line = cab.ProductNumber,
                                Qty = cab.Qty,
                                Height = cab.Height,
                                Width = cab.Width,
                                Depth = cab.Depth,
                                Description = cab.GetDescription(),
                                UnitPrice = cab.UnitPrice,
                            }).ToList(),
            CounterTops = order.Products
                            .OfType<CounterTop>()
                            .Select(c => new CounterTopItem() {
                                Line = c.ProductNumber,
                                Qty = c.Qty,
                                Finish = c.Finish,
                                Width = c.Width,
                                Length = c.Length,
                                UnitPrice = c.UnitPrice,
                            }).ToList(),
            AdditionalItems = order.AdditionalItems
                            .Select((item, idx) => new AdditionalItem() {
                                Line = idx + 1,
                                Qty = item.Qty,
                                Description = item.Description,
                                UnitPrice = item.UnitPrice
                            }).ToList()
        };

    }

}
