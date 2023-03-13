using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.PackingList;

internal class PackingListDecorator : IDocumentDecorator {

    public void Decorate(Order order, IDocumentContainer container) {

        container.Page(page => {

            page.Content().Text($"Packing List {order.Number} {order.Name}");

        });

    }

}
