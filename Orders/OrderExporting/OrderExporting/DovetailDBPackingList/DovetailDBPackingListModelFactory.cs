using Domain.Companies;
using Domain.Companies.Entities;
using Domain.Orders;
using Domain.Orders.Builders;
using Domain.Orders.Entities;

namespace OrderExporting.DovetailDBPackingList;

public class DovetailDBPackingListModelFactory {

    public static DovetailDrawerBoxPackingList CreateDBPackingList(Order order, Vendor vendor, Customer customer) {

        return new() {
            OrderDate = order.OrderDate,
            OrderName = order.Name,
            OrderNumber = order.Number,
            Customer = new() {
                Name = customer?.Name ?? "",
                Line1 = order.Shipping.Address.Line1,
                Line2 = order.Shipping.Address.Line2,
                Line3 = order.Shipping.Address.GetLine4(),
                Line4 = order.Shipping.PhoneNumber,
            },
            Vendor = new() {
                Name = vendor?.Name ?? "",
                Line1 = vendor?.Address.Line1 ?? "",
                Line2 = vendor?.Address.Line2 ?? "",
                Line3 = vendor is null ? "" : vendor.Address.GetLine4(),
                Line4 = vendor?.Phone ?? "",
            },
            Items = order.Products
                            .OfType<IDovetailDrawerBoxContainer>()
                            .Where(p => p.ContainsDovetailDrawerBoxes())
                            .SelectMany(p => p.GetDovetailDrawerBoxes(() => new()))
                            .Select(db => new DovetailDrawerBox() {
                                Line = db.ProductNumber,
                                Qty = db.Qty,
                                Height = db.Height,
                                Width = db.Width,
                                Depth = db.Depth,
                                Description = db.GetDescription()
                            })
                            .ToList()
        };

    }

}
