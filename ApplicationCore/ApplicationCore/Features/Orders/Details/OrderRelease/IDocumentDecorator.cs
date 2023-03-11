using QuestPDF.Infrastructure;

namespace ApplicationCore.Features.Orders.Details.OrderRelease;

internal interface IDocumentDecorator {

    public void Decorate(IDocumentContainer container);

}