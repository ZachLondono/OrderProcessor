using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using QuestPDF.Infrastructure;

namespace ApplicationCore.Features.Orders.Details.OrderRelease;

public interface IDocumentDecorator {

    public Task Decorate(Order order, IDocumentContainer container);

}