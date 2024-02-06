using Domain.Orders.Entities;
using ApplicationCore.Shared;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.Invoice;

public interface IInvoiceDecorator : IDocumentDecorator {

    public Task AddData(Order order);

}
