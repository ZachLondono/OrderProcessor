using QuestPDF.Infrastructure;

namespace ApplicationCore.Features.Orders.Details.Shared;

internal interface IDocumentDecorator {

    public void Decorate(IDocumentContainer container);

}
