using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using QuestPDF.Infrastructure;

namespace ApplicationCore.Features.Orders.Details.OrderRelease;

internal interface IDocumentDecorator {

    public void Decorate(Order order, IDocumentContainer container);

}