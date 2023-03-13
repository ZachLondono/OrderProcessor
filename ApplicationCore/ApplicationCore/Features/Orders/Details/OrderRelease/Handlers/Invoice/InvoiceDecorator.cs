using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.Invoice;

internal class InvoiceDecorator : IDocumentDecorator {

    public void Decorate(Order order, IDocumentContainer container) {

        container.Page(page => {

            page.Content().Text($"Invoice {order.Number} {order.Name}");

        });

    }

}
