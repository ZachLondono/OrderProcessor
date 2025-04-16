using Companies.Vendors.Queries;
using Domain.Companies.Entities;
using Domain.Orders.Entities;
using MediatR;
using OrderExporting.Invoice;
using QuestPDF.Fluent;

namespace ApplicationCore.Features.VendorInvoice;

public class VendorInvoiceService {

    private readonly ISender _sender;

    public VendorInvoiceService(ISender sender) {
        _sender = sender;
    }

    public async Task<string> CreateInvoice(Guid orderId) {

        var order = await GetOrder(orderId);

        var vendor = await GetVendor(order.VendorId);
        var royal = await GetVendor(Guid.Parse("579badff-4579-481d-98cf-0012eb2cc75e"));

        var model = InvoiceModelFactory.CreateInvoiceModel(order, royal, vendor.Name);
        model.Customer.Line1 = vendor.Address.Line1;
        model.Customer.Line2 = vendor.Address.Line2;
        model.Customer.Line3 = vendor.Address.GetLine4();
        model.Customer.Line4 = vendor.Phone;
        model.Terms = "";

        var document = Document.Create(d => {
            var decorator = new InvoiceDecorator(model);
            decorator.Decorate(d);
        });

        var filePath = Path.Combine(order.WorkingDirectory, $"{order.Number} Invoice.pdf");

        document.GeneratePdf(filePath);

        return filePath;

    }

    private async Task<Vendor> GetVendor(Guid vendorId) {

        var response = await _sender.Send(new GetVendorById.Query(vendorId));

        Vendor? vendor = null;
        response.Match(
            v => vendor = v,
            e => throw new Exception($"{e.Title} - {e.Details}"));

        return vendor!;

    }

    private async Task<Order> GetOrder(Guid orderId) {

        var response = await _sender.Send(new Orders.Details.Queries.GetOrderById.Query(orderId));

        Order? order = null;
        response.Match(
            o => order = o,
            e => throw new Exception($"{e.Title} - {e.Details}"));

        return order!;

    }

}
