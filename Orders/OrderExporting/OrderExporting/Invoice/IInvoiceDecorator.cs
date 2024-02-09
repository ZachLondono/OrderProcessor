using Domain.Orders.Entities;
using OrderExporting.Shared;

namespace OrderExporting.Invoice;

public interface IInvoiceDecorator : IDocumentDecorator {

    public Task AddData(Order order);

}
