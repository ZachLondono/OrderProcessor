using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Products.DrawerBoxes;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.DovetailDBPackingList;

public class DovetailDBPackingListDecoratorFactory {

    private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerByIdAsync;
    private readonly CompanyDirectory.GetVendorByIdAsync _getVendorByIdAsync;

    public DovetailDBPackingListDecoratorFactory(CompanyDirectory.GetVendorByIdAsync getVendorByIdAsync, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync) {
        _getVendorByIdAsync = getVendorByIdAsync;
        _getCustomerByIdAsync = getCustomerByIdAsync;
    }

    public async Task<DovetailDBPackingListDecorator> CreateDecorator(Order order) {

        var customer = await _getCustomerByIdAsync(order.CustomerId);
        var vendor = await _getVendorByIdAsync(order.VendorId);

        return new() {
            Data = new() {
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
                            .Where(p => p is DovetailDrawerBoxProduct)
                            .Cast<DovetailDrawerBoxProduct>()
                            .Select(db => new DovetailDrawerBox() {
                                Line = db.ProductNumber,
                                Qty = db.Qty,
                                Height = db.Height,
                                Width = db.Width,
                                Depth = db.Depth
                            })
                            .ToList()
            }
        };

    } 

}
