using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Shared;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.Invoice;

public interface IInvoiceDecorator : IDocumentDecorator {

    public Task AddData(Order order);

}
